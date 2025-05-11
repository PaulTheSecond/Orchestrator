using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrchestratorApp.Data;
using OrchestratorApp.Domain.Entities;
using OrchestratorApp.Domain.Enums;
using OrchestratorApp.Domain.Messaging;
using OrchestratorApp.DTOs;

namespace OrchestratorApp.Services
{
    public class ContestInstanceService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly RabbitMQService _rabbitMQService;
        private readonly ILogger<ContestInstanceService> _logger;

        public ContestInstanceService(
            ApplicationDbContext dbContext,
            RabbitMQService rabbitMQService,
            ILogger<ContestInstanceService> logger)
        {
            _dbContext = dbContext;
            _rabbitMQService = rabbitMQService;
            _logger = logger;
        }

        public async Task<IEnumerable<ContestInstanceDTO>> GetAllContestInstancesAsync()
        {
            var contestInstances = await _dbContext.ContestInstances
                .Include(c => c.CurrentStage)
                .AsNoTracking()
                .ToListAsync();

            return contestInstances.Select(MapToDTO);
        }

        public async Task<ContestInstanceDTO?> GetContestInstanceAsync(Guid id)
        {
            var contestInstance = await _dbContext.ContestInstances
                .Include(c => c.CurrentStage)
                .Include(c => c.StageConfigurations)
                    .ThenInclude(sc => sc.StageTemplate)
                .Include(c => c.ApplicationInstances)
                    .ThenInclude(a => a.CurrentStage)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contestInstance == null)
            {
                return null;
            }

            return MapToDTO(contestInstance);
        }

        public async Task<IEnumerable<ContestInstanceDTO>> GetContestInstancesByProcedureAsync(Guid procedureInstanceId)
        {
            var contestInstances = await _dbContext.ContestInstances
                .Include(c => c.CurrentStage)
                .Include(c => c.StageConfigurations)
                    .ThenInclude(sc => sc.StageTemplate)
                .Where(c => c.ProcedureInstanceId == procedureInstanceId)
                .AsNoTracking()
                .ToListAsync();

            return contestInstances.Select(MapToDTO);
        }

        public async Task<ContestInstanceDTO> CreateContestInstanceAsync(CreateContestInstanceDTO dto)
        {
            // Validate the procedure instance is in the Contests stage
            var procedureInstance = await _dbContext.ProcedureInstances
                .Include(p => p.CurrentStage)
                .FirstOrDefaultAsync(p => p.Id == dto.ProcedureInstanceId);

            if (procedureInstance == null)
            {
                throw new ArgumentException($"Procedure instance with ID {dto.ProcedureInstanceId} not found");
            }

            if (procedureInstance.CurrentStage?.StageType != ProcedureStageType.Contests)
            {
                throw new InvalidOperationException("Procedure instance is not in the Contests stage");
            }

            // Get the contest template
            var contestTemplate = await _dbContext.ContestTemplates
                .Include(c => c.Stages)
                .FirstOrDefaultAsync(c => c.Id == dto.ContestTemplateId && c.IsPublished);

            if (contestTemplate == null)
            {
                throw new ArgumentException($"Published contest template with ID {dto.ContestTemplateId} not found");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Create contest instance
                var contestInstance = new ContestInstance
                {
                    Id = Guid.NewGuid(),
                    ProcedureInstanceId = procedureInstance.Id,
                    ContestTemplateId = contestTemplate.Id,
                    ContestTemplateVersion = contestTemplate.Version,
                    Name = dto.Name,
                    Status = contestTemplate.StatusModelArray.FirstOrDefault() ?? "NotStarted",
                    StartDate = DateTime.UtcNow
                };

                // Set the first stage if available
                var firstStage = contestTemplate.Stages.OrderBy(s => s.Order).FirstOrDefault();
                if (firstStage != null)
                {
                    contestInstance.CurrentStageId = firstStage.Id;
                }

                await _dbContext.ContestInstances.AddAsync(contestInstance);
                await _dbContext.SaveChangesAsync();

                // Create stage configurations
                foreach (var configDto in dto.StageConfigurations)
                {
                    // Validate stage belongs to this contest template
                    var stageTemplate = contestTemplate.Stages.FirstOrDefault(s => s.Id == configDto.StageTemplateId);
                    if (stageTemplate == null)
                    {
                        throw new ArgumentException($"Stage with ID {configDto.StageTemplateId} does not belong to the contest template");
                    }

                    var stageConfig = new StageConfiguration
                    {
                        Id = Guid.NewGuid(),
                        ContestInstanceId = contestInstance.Id,
                        StageTemplateId = configDto.StageTemplateId,
                        StartDate = configDto.StartDate,
                        EndDate = configDto.EndDate,
                        ServiceName = string.IsNullOrEmpty(configDto.ServiceName) ? stageTemplate.DefaultServiceName : configDto.ServiceName
                    };

                    await _dbContext.StageConfigurations.AddAsync(stageConfig);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // If a stage is set, send event to start it
                if (contestInstance.CurrentStageId.HasValue && firstStage != null)
                {
                    // Get the stage configuration for the first stage
                    var stageConfig = await _dbContext.StageConfigurations
                        .FirstOrDefaultAsync(sc => sc.ContestInstanceId == contestInstance.Id && sc.StageTemplateId == firstStage.Id);

                    string serviceName = stageConfig?.ServiceName ?? firstStage.DefaultServiceName;

                    await _rabbitMQService.PublishEventAsync(new StageStartedEvent
                    {
                        ProcedureInstanceId = procedureInstance.Id,
                        ContestInstanceId = contestInstance.Id,
                        StageId = contestInstance.CurrentStageId.Value,
                        StageType = firstStage.StageType.ToString(),
                        ServiceName = serviceName
                    });
                }

                return await GetContestInstanceAsync(contestInstance.Id) ?? 
                    throw new Exception("Failed to retrieve the created contest instance");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating contest instance");
                throw;
            }
        }

        public async Task<ContestInstanceDTO> InterruptContestAsync(InterruptContestDTO dto)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var contestInstance = await _dbContext.ContestInstances
                    .Include(c => c.ApplicationInstances)
                    .FirstOrDefaultAsync(c => c.Id == dto.ContestInstanceId);

                if (contestInstance == null)
                {
                    throw new KeyNotFoundException($"Contest instance with ID {dto.ContestInstanceId} not found");
                }

                // Check if the status is valid for interruption
                var contestTemplate = await _dbContext.ContestTemplates
                    .Where(ct => ct.Id == contestInstance.ContestTemplateId)
                    .FirstOrDefaultAsync();

                if (contestTemplate == null || !contestTemplate.StatusModelArray.Contains("Interrupted"))
                {
                    throw new InvalidOperationException("Contest template does not support 'Interrupted' status");
                }

                // Interrupt the contest
                contestInstance.Status = "Interrupted";
                contestInstance.EndDate = DateTime.UtcNow;

                // Mark all applications as interrupted
                foreach (var application in contestInstance.ApplicationInstances)
                {
                    application.Status = "Interrupted";
                    application.UpdatedAt = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync();

                // Send interrupt event
                await _rabbitMQService.PublishEventAsync(new InterruptContestEvent
                {
                    ContestInstanceId = contestInstance.Id,
                    Reason = dto.Reason
                });

                await transaction.CommitAsync();

                return await GetContestInstanceAsync(contestInstance.Id) ?? 
                    throw new Exception("Failed to retrieve the interrupted contest instance");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error interrupting contest instance");
                throw;
            }
        }

        public async Task<ContestInstanceDTO> UpdateContestTemplateVersionAsync(Guid contestInstanceId, int newVersion)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var contestInstance = await _dbContext.ContestInstances
                    .Include(c => c.StageConfigurations)
                    .Include(c => c.ContestTemplate)
                        .ThenInclude(ct => ct.Stages)
                    .FirstOrDefaultAsync(c => c.Id == contestInstanceId);

                if (contestInstance == null)
                {
                    throw new KeyNotFoundException($"Contest instance with ID {contestInstanceId} not found");
                }

                // Get the new template version
                var newTemplateVersion = await _dbContext.ContestTemplates
                    .Include(ct => ct.Stages)
                    .FirstOrDefaultAsync(ct => ct.Id == contestInstance.ContestTemplateId && 
                                              ct.Version == newVersion && 
                                              ct.IsPublished);

                if (newTemplateVersion == null)
                {
                    throw new ArgumentException($"Published contest template with ID {contestInstance.ContestTemplateId} and version {newVersion} not found");
                }

                // Update the contest instance
                contestInstance.ContestTemplateVersion = newVersion;

                // Update stage configurations based on new template
                foreach (var stageConfig in contestInstance.StageConfigurations)
                {
                    // Find the corresponding stage in the new template
                    var oldStage = contestInstance.ContestTemplate.Stages
                        .FirstOrDefault(s => s.Id == stageConfig.StageTemplateId);

                    if (oldStage == null) continue;

                    // Find the corresponding stage in the new template by stage type
                    var newStage = newTemplateVersion.Stages
                        .FirstOrDefault(s => s.StageType == oldStage.StageType);

                    if (newStage == null) continue;

                    // Update the stage template ID in the configuration
                    stageConfig.StageTemplateId = newStage.Id;

                    // Update service name if it was using the default
                    if (stageConfig.ServiceName == oldStage.DefaultServiceName)
                    {
                        stageConfig.ServiceName = newStage.DefaultServiceName;
                    }
                }

                // If the current stage exists in the old template, find its equivalent in the new template
                if (contestInstance.CurrentStageId.HasValue)
                {
                    var currentStage = contestInstance.ContestTemplate.Stages
                        .FirstOrDefault(s => s.Id == contestInstance.CurrentStageId.Value);

                    if (currentStage != null)
                    {
                        // Find the corresponding stage in the new template by stage type
                        var newCurrentStage = newTemplateVersion.Stages
                            .FirstOrDefault(s => s.StageType == currentStage.StageType);

                        if (newCurrentStage != null)
                        {
                            contestInstance.CurrentStageId = newCurrentStage.Id;
                        }
                    }
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetContestInstanceAsync(contestInstance.Id) ?? 
                    throw new Exception("Failed to retrieve the updated contest instance");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating contest template version");
                throw;
            }
        }

        private ContestInstanceDTO MapToDTO(ContestInstance entity)
        {
            var dto = new ContestInstanceDTO
            {
                Id = entity.Id,
                ProcedureInstanceId = entity.ProcedureInstanceId,
                ContestTemplateId = entity.ContestTemplateId,
                ContestTemplateVersion = entity.ContestTemplateVersion,
                Name = entity.Name,
                CurrentStageId = entity.CurrentStageId,
                Status = entity.Status,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate
            };

            if (entity.StageConfigurations != null)
            {
                dto.StageConfigurations = entity.StageConfigurations.Select(sc => new StageConfigurationDTO
                {
                    Id = sc.Id,
                    ContestInstanceId = sc.ContestInstanceId,
                    StageTemplateId = sc.StageTemplateId,
                    StartDate = sc.StartDate,
                    EndDate = sc.EndDate,
                    ServiceName = sc.ServiceName
                }).ToList();
            }

            if (entity.ApplicationInstances != null)
            {
                dto.ApplicationInstances = entity.ApplicationInstances.Select(a => new ApplicationInstanceDTO
                {
                    Id = a.Id,
                    ContestInstanceId = a.ContestInstanceId,
                    CurrentStageId = a.CurrentStageId,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    ExternalApplicationId = a.ExternalApplicationId
                }).ToList();
            }

            return dto;
        }
    }
}

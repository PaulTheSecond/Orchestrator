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
    public class ApplicationInstanceService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly RabbitMQService _rabbitMQService;
        private readonly ILogger<ApplicationInstanceService> _logger;

        public ApplicationInstanceService(
            ApplicationDbContext dbContext,
            RabbitMQService rabbitMQService,
            ILogger<ApplicationInstanceService> logger)
        {
            _dbContext = dbContext;
            _rabbitMQService = rabbitMQService;
            _logger = logger;
        }

        public async Task<IEnumerable<ApplicationInstanceDTO>> GetApplicationsByContestAsync(Guid contestInstanceId)
        {
            var applications = await _dbContext.ApplicationInstances
                .Include(a => a.CurrentStage)
                .Include(a => a.StageResults)
                    .ThenInclude(sr => sr.StageTemplate)
                .Where(a => a.ContestInstanceId == contestInstanceId)
                .AsNoTracking()
                .ToListAsync();

            return applications.Select(MapToDTO);
        }

        public async Task<ApplicationInstanceDTO?> GetApplicationInstanceAsync(Guid id)
        {
            var application = await _dbContext.ApplicationInstances
                .Include(a => a.CurrentStage)
                .Include(a => a.StageResults)
                    .ThenInclude(sr => sr.StageTemplate)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
            {
                return null;
            }

            return MapToDTO(application);
        }

        public async Task<ApplicationInstanceDTO> CreateApplicationInstanceAsync(CreateApplicationDTO dto)
        {
            // Validate contest instance
            var contestInstance = await _dbContext.ContestInstances
                .Include(c => c.ContestTemplate)
                    .ThenInclude(ct => ct.Stages)
                .FirstOrDefaultAsync(c => c.Id == dto.ContestInstanceId);

            if (contestInstance == null)
            {
                throw new ArgumentException($"Contest instance with ID {dto.ContestInstanceId} not found");
            }

            if (contestInstance.Status == "Completed" || contestInstance.Status == "Interrupted")
            {
                throw new InvalidOperationException("Cannot create application for a completed or interrupted contest");
            }

            // Find the first stage for the application
            var firstStage = contestInstance.ContestTemplate.Stages
                .OrderBy(s => s.Order)
                .FirstOrDefault();

            if (firstStage == null)
            {
                throw new InvalidOperationException("Contest template has no stages defined");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Create application instance
                var application = new ApplicationInstance
                {
                    Id = Guid.NewGuid(),
                    ContestInstanceId = contestInstance.Id,
                    CurrentStageId = firstStage.Id,
                    Status = "Submitted",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ExternalApplicationId = dto.ExternalApplicationId
                };

                await _dbContext.ApplicationInstances.AddAsync(application);
                await _dbContext.SaveChangesAsync();

                // Get the stage configuration for the first stage to find the service name
                var stageConfig = await _dbContext.StageConfigurations
                    .FirstOrDefaultAsync(sc => sc.ContestInstanceId == contestInstance.Id && sc.StageTemplateId == firstStage.Id);

                string serviceName = stageConfig?.ServiceName ?? firstStage.DefaultServiceName;

                // Get procedure instance ID for the event
                var procedureInstanceId = await _dbContext.ContestInstances
                    .Where(c => c.Id == contestInstance.Id)
                    .Select(c => c.ProcedureInstanceId)
                    .FirstOrDefaultAsync();

                // Send event to start the first stage
                await _rabbitMQService.PublishEventAsync(new StageStartedEvent
                {
                    ProcedureInstanceId = procedureInstanceId,
                    ContestInstanceId = contestInstance.Id,
                    ApplicationInstanceId = application.Id,
                    StageId = firstStage.Id,
                    StageType = firstStage.StageType.ToString(),
                    ServiceName = serviceName
                });

                await transaction.CommitAsync();

                return await GetApplicationInstanceAsync(application.Id) ?? 
                    throw new Exception("Failed to retrieve the created application instance");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating application instance");
                throw;
            }
        }

        public async Task<ApplicationInstanceDTO> AddStageResultAsync(Guid applicationId, Guid stageId, ResultStatus resultStatus, string? resultData, Guid integrationEventId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var application = await _dbContext.ApplicationInstances
                    .Include(a => a.CurrentStage)
                    .FirstOrDefaultAsync(a => a.Id == applicationId);

                if (application == null)
                {
                    throw new KeyNotFoundException($"Application instance with ID {applicationId} not found");
                }

                if (application.CurrentStageId != stageId)
                {
                    throw new InvalidOperationException($"Stage ID {stageId} does not match the current stage {application.CurrentStageId} for application {applicationId}");
                }

                // Check if this result has already been processed (idempotency)
                bool resultExists = await _dbContext.StageResults
                    .AnyAsync(sr => sr.IntegrationEventId == integrationEventId);

                if (resultExists)
                {
                    _logger.LogWarning("Stage result with integration event ID {EventId} has already been processed, ignoring duplicate", integrationEventId);
                    
                    // Return the current state without making changes
                    return await GetApplicationInstanceAsync(applicationId) ?? 
                        throw new Exception("Failed to retrieve the application instance");
                }

                // Add the stage result
                var stageResult = new StageResult
                {
                    Id = Guid.NewGuid(),
                    ApplicationInstanceId = applicationId,
                    StageTemplateId = stageId,
                    ResultStatus = resultStatus,
                    ResultData = resultData,
                    CompletedAt = DateTime.UtcNow,
                    IntegrationEventId = integrationEventId
                };

                await _dbContext.StageResults.AddAsync(stageResult);

                // Update application status based on result
                application.UpdatedAt = DateTime.UtcNow;

                if (resultStatus == ResultStatus.Rejected)
                {
                    application.Status = "Rejected";
                    application.CurrentStageId = null;
                }
                else if (resultStatus == ResultStatus.Failed)
                {
                    application.Status = "Failed";
                    application.CurrentStageId = null;
                }
                else // Success
                {
                    // Get the current stage to find the next stage
                    var currentStage = await _dbContext.StageTemplates
                        .FirstOrDefaultAsync(s => s.Id == stageId);

                    if (currentStage?.NextStageId != null)
                    {
                        // Move to the next stage
                        application.CurrentStageId = currentStage.NextStageId;
                        application.Status = "InProgress";
                    }
                    else
                    {
                        // This was the last stage, mark as approved
                        application.Status = "Approved";
                        application.CurrentStageId = null;
                    }
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetApplicationInstanceAsync(applicationId) ?? 
                    throw new Exception("Failed to retrieve the updated application instance");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error adding stage result for application instance");
                throw;
            }
        }

        public async Task<ApplicationInstanceDTO> MoveToNextStageAsync(Guid applicationId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var application = await _dbContext.ApplicationInstances
                    .Include(a => a.CurrentStage)
                    .FirstOrDefaultAsync(a => a.Id == applicationId);

                if (application == null)
                {
                    throw new KeyNotFoundException($"Application instance with ID {applicationId} not found");
                }

                if (application.CurrentStageId == null)
                {
                    throw new InvalidOperationException($"Application {applicationId} has no current stage to advance from");
                }

                // Get the current stage to find the next stage
                var currentStage = await _dbContext.StageTemplates
                    .FirstOrDefaultAsync(s => s.Id == application.CurrentStageId);

                if (currentStage?.NextStageId == null)
                {
                    throw new InvalidOperationException($"Current stage {currentStage?.Id} has no next stage defined");
                }

                // Get the contest instance and procedure instance IDs
                var contestInstance = await _dbContext.ContestInstances
                    .FirstOrDefaultAsync(c => c.Id == application.ContestInstanceId);

                if (contestInstance == null)
                {
                    throw new InvalidOperationException($"Contest instance for application {applicationId} not found");
                }

                // Get the next stage
                var nextStage = await _dbContext.StageTemplates
                    .FirstOrDefaultAsync(s => s.Id == currentStage.NextStageId);

                if (nextStage == null)
                {
                    throw new InvalidOperationException($"Next stage with ID {currentStage.NextStageId} not found");
                }

                // Get the stage configuration for the next stage to find the service name
                var stageConfig = await _dbContext.StageConfigurations
                    .FirstOrDefaultAsync(sc => sc.ContestInstanceId == contestInstance.Id && sc.StageTemplateId == nextStage.Id);

                string serviceName = stageConfig?.ServiceName ?? nextStage.DefaultServiceName;

                // Update application status
                application.CurrentStageId = nextStage.Id;
                application.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                // Send event to start the next stage
                await _rabbitMQService.PublishEventAsync(new StageStartedEvent
                {
                    ProcedureInstanceId = contestInstance.ProcedureInstanceId,
                    ContestInstanceId = contestInstance.Id,
                    ApplicationInstanceId = application.Id,
                    StageId = nextStage.Id,
                    StageType = nextStage.StageType.ToString(),
                    ServiceName = serviceName
                });

                await transaction.CommitAsync();

                return await GetApplicationInstanceAsync(applicationId) ?? 
                    throw new Exception("Failed to retrieve the updated application instance");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error moving application instance to next stage");
                throw;
            }
        }

        private ApplicationInstanceDTO MapToDTO(ApplicationInstance entity)
        {
            var dto = new ApplicationInstanceDTO
            {
                Id = entity.Id,
                ContestInstanceId = entity.ContestInstanceId,
                CurrentStageId = entity.CurrentStageId,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                ExternalApplicationId = entity.ExternalApplicationId
            };

            if (entity.StageResults != null)
            {
                dto.StageResults = entity.StageResults.Select(sr => new StageResultDTO
                {
                    Id = sr.Id,
                    ApplicationInstanceId = sr.ApplicationInstanceId,
                    StageTemplateId = sr.StageTemplateId,
                    ResultStatus = sr.ResultStatus,
                    ResultData = sr.ResultData,
                    CompletedAt = sr.CompletedAt,
                    IntegrationEventId = sr.IntegrationEventId
                }).ToList();
            }

            return dto;
        }
    }
}

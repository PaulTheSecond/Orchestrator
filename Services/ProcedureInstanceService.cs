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
    public class ProcedureInstanceService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly RabbitMQService _rabbitMQService;
        private readonly ILogger<ProcedureInstanceService> _logger;

        public ProcedureInstanceService(
            ApplicationDbContext dbContext,
            RabbitMQService rabbitMQService,
            ILogger<ProcedureInstanceService> logger)
        {
            _dbContext = dbContext;
            _rabbitMQService = rabbitMQService;
            _logger = logger;
        }

        public async Task<IEnumerable<ProcedureInstanceDTO>> GetAllProcedureInstancesAsync()
        {
            var procedureInstances = await _dbContext.ProcedureInstances
                .Include(p => p.CurrentStage)
                .AsNoTracking()
                .ToListAsync();

            return procedureInstances.Select(MapToDTO);
        }

        public async Task<ProcedureInstanceDTO?> GetProcedureInstanceAsync(Guid id)
        {
            var procedureInstance = await _dbContext.ProcedureInstances
                .Include(p => p.CurrentStage)
                .Include(p => p.ContestInstances)
                    .ThenInclude(c => c.CurrentStage)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (procedureInstance == null)
            {
                return null;
            }

            return MapToDTO(procedureInstance);
        }

        public async Task<ProcedureInstanceDTO> CreateProcedureInstanceAsync(CreateProcedureInstanceDTO dto)
        {
            // Get the procedure template
            var procedureTemplate = await _dbContext.ProcedureTemplates
                .Include(p => p.ProcedureStages.OrderBy(s => s.Order))
                .FirstOrDefaultAsync(p => p.Id == dto.ProcedureTemplateId && p.IsPublished);

            if (procedureTemplate == null)
            {
                throw new ArgumentException($"Published procedure template with ID {dto.ProcedureTemplateId} not found");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Create procedure instance
                var procedureInstance = new ProcedureInstance
                {
                    Id = Guid.NewGuid(),
                    ProcedureTemplateId = procedureTemplate.Id,
                    TemplateVersion = procedureTemplate.Version,
                    Name = dto.Name,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Set the first stage
                var firstStage = procedureTemplate.ProcedureStages.FirstOrDefault();
                if (firstStage != null)
                {
                    procedureInstance.CurrentStageId = firstStage.Id;
                }

                await _dbContext.ProcedureInstances.AddAsync(procedureInstance);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // If a stage is set, send event to start it
                if (procedureInstance.CurrentStageId.HasValue)
                {
                    await _rabbitMQService.PublishEventAsync(new StageStartedEvent
                    {
                        ProcedureInstanceId = procedureInstance.Id,
                        StageId = procedureInstance.CurrentStageId.Value,
                        StageType = firstStage?.StageType.ToString() ?? string.Empty,
                        ServiceName = firstStage?.DefaultServiceName ?? string.Empty
                    });
                }

                return await GetProcedureInstanceAsync(procedureInstance.Id) ?? 
                    throw new Exception("Failed to retrieve the created procedure instance");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating procedure instance");
                throw;
            }
        }

        public async Task<ProcedureInstanceDTO> TransitionToNextStageAsync(Guid id)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var procedureInstance = await _dbContext.ProcedureInstances
                    .Include(p => p.CurrentStage)
                    .Include(p => p.ContestInstances)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (procedureInstance == null)
                {
                    throw new KeyNotFoundException($"Procedure instance with ID {id} not found");
                }

                if (procedureInstance.Status != "Active")
                {
                    throw new InvalidOperationException($"Cannot transition procedure instance with status {procedureInstance.Status}");
                }

                // Get the current stage
                var currentStage = procedureInstance.CurrentStage;
                if (currentStage == null)
                {
                    throw new InvalidOperationException("Procedure instance has no current stage");
                }

                // If this is the Contests stage, check if all contests are completed
                if (currentStage.StageType == ProcedureStageType.Contests)
                {
                    var hasActiveContests = procedureInstance.ContestInstances
                        .Any(c => c.Status != "Completed" && c.Status != "Interrupted");

                    if (hasActiveContests)
                    {
                        throw new InvalidOperationException("Cannot proceed to the next stage while there are active contests");
                    }
                }

                // Get the next stage
                if (currentStage.NextStageId == null)
                {
                    // This is the last stage, mark the procedure as completed
                    procedureInstance.Status = "Completed";
                    procedureInstance.CurrentStageId = null;
                    procedureInstance.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Get the next stage from the database
                    var nextStage = await _dbContext.ProcedureStageTemplates
                        .FirstOrDefaultAsync(s => s.Id == currentStage.NextStageId);

                    if (nextStage == null)
                    {
                        throw new InvalidOperationException("Next stage not found");
                    }

                    // Update procedure instance
                    procedureInstance.CurrentStageId = nextStage.Id;
                    procedureInstance.UpdatedAt = DateTime.UtcNow;

                    // Create event for stage transition
                    await _rabbitMQService.PublishEventAsync(new ProcedureStageTransitionEvent
                    {
                        ProcedureInstanceId = procedureInstance.Id,
                        CurrentStageId = currentStage.Id,
                        NextStageId = nextStage.Id
                    });

                    // Send event to start the next stage
                    await _rabbitMQService.PublishEventAsync(new StageStartedEvent
                    {
                        ProcedureInstanceId = procedureInstance.Id,
                        StageId = nextStage.Id,
                        StageType = nextStage.StageType.ToString(),
                        ServiceName = nextStage.DefaultServiceName ?? string.Empty
                    });
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetProcedureInstanceAsync(procedureInstance.Id) ?? 
                    throw new Exception("Failed to retrieve the updated procedure instance");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error transitioning procedure stage");
                throw;
            }
        }

        private ProcedureInstanceDTO MapToDTO(ProcedureInstance entity)
        {
            var dto = new ProcedureInstanceDTO
            {
                Id = entity.Id,
                ProcedureTemplateId = entity.ProcedureTemplateId,
                TemplateVersion = entity.TemplateVersion,
                Name = entity.Name,
                CurrentStageId = entity.CurrentStageId,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };

            if (entity.ContestInstances != null)
            {
                dto.ContestInstances = entity.ContestInstances.Select(c => new ContestInstanceDTO
                {
                    Id = c.Id,
                    ProcedureInstanceId = c.ProcedureInstanceId,
                    ContestTemplateId = c.ContestTemplateId,
                    ContestTemplateVersion = c.ContestTemplateVersion,
                    Name = c.Name,
                    CurrentStageId = c.CurrentStageId,
                    Status = c.Status,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate
                }).ToList();
            }

            return dto;
        }
    }
}

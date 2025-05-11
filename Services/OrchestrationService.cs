using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrchestratorApp.Data;
using OrchestratorApp.Domain.Entities;
using OrchestratorApp.Domain.Messaging;

namespace OrchestratorApp.Services
{
    public class OrchestrationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly RabbitMQService _rabbitMQService;
        private readonly ApplicationInstanceService _applicationInstanceService;
        private readonly ContestInstanceService _contestInstanceService;
        private readonly ILogger<OrchestrationService> _logger;

        public OrchestrationService(
            ApplicationDbContext dbContext,
            RabbitMQService rabbitMQService,
            ApplicationInstanceService applicationInstanceService,
            ContestInstanceService contestInstanceService,
            ILogger<OrchestrationService> logger)
        {
            _dbContext = dbContext;
            _rabbitMQService = rabbitMQService;
            _applicationInstanceService = applicationInstanceService;
            _contestInstanceService = contestInstanceService;
            _logger = logger;
        }

        public async Task HandleCreateApplicationEventAsync(CreateApplicationEvent ev, Guid integrationEventId)
        {
            try
            {
                _logger.LogInformation("Processing CreateApplicationEvent for contest {ContestId} with integration event ID {EventId}",
                    ev.ContestInstanceId, integrationEventId);

                await _applicationInstanceService.CreateApplicationInstanceAsync(new DTOs.CreateApplicationDTO
                {
                    ContestInstanceId = ev.ContestInstanceId,
                    ExternalApplicationId = ev.ExternalApplicationId,
                    ApplicationData = ev.ApplicationData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling CreateApplicationEvent with ID {EventId}", integrationEventId);
                throw;
            }
        }

        public async Task HandleStageCompletedEventAsync(StageCompletedEvent ev, Guid integrationEventId)
        {
            try
            {
                _logger.LogInformation("Processing StageCompletedEvent for application {ApplicationId} stage {StageId} with integration event ID {EventId}",
                    ev.ApplicationInstanceId, ev.StageId, integrationEventId);

                // Process the stage result for the application
                var updatedApplication = await _applicationInstanceService.AddStageResultAsync(
                    ev.ApplicationInstanceId,
                    ev.StageId,
                    ev.ResultStatus,
                    ev.ResultData,
                    integrationEventId);

                // If the application moved to a new stage, start that stage
                if (updatedApplication.CurrentStageId.HasValue)
                {
                    await _applicationInstanceService.MoveToNextStageAsync(ev.ApplicationInstanceId);
                }

                // Check if this affects the contest status
                await CheckAndUpdateContestStatusAsync(updatedApplication.ContestInstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling StageCompletedEvent with ID {EventId}", integrationEventId);
                throw;
            }
        }

        public async Task HandleInterruptContestEventAsync(InterruptContestEvent ev, Guid integrationEventId)
        {
            try
            {
                _logger.LogInformation("Processing InterruptContestEvent for contest {ContestId} with integration event ID {EventId}",
                    ev.ContestInstanceId, integrationEventId);

                // Interrupt the contest
                await _contestInstanceService.InterruptContestAsync(new DTOs.InterruptContestDTO
                {
                    ContestInstanceId = ev.ContestInstanceId,
                    Reason = ev.Reason
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling InterruptContestEvent with ID {EventId}", integrationEventId);
                throw;
            }
        }

        public async Task HandleContestTemplateUpdatedEventAsync(ContestTemplateUpdatedEvent ev, Guid integrationEventId)
        {
            try
            {
                _logger.LogInformation("Processing ContestTemplateUpdatedEvent for template {TemplateId} version {Version} with integration event ID {EventId}",
                    ev.ContestTemplateId, ev.NewVersion, integrationEventId);

                // Find all active contest instances using this template
                var contestInstances = await _dbContext.ContestInstances
                    .Where(c => c.ContestTemplateId == ev.ContestTemplateId && 
                                c.Status != "Completed" && 
                                c.Status != "Interrupted" &&
                                c.ContestTemplateVersion != ev.NewVersion)
                    .ToListAsync();

                // Update each instance to use the new template version
                foreach (var contest in contestInstances)
                {
                    await _contestInstanceService.UpdateContestTemplateVersionAsync(contest.Id, ev.NewVersion);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ContestTemplateUpdatedEvent with ID {EventId}", integrationEventId);
                throw;
            }
        }

        public async Task HandleProcedureStageTransitionEventAsync(ProcedureStageTransitionEvent ev, Guid integrationEventId)
        {
            try
            {
                _logger.LogInformation("Processing ProcedureStageTransitionEvent for procedure {ProcedureId} with integration event ID {EventId}",
                    ev.ProcedureInstanceId, integrationEventId);

                // Get the procedure instance
                var procedureInstance = await _dbContext.ProcedureInstances
                    .Include(p => p.CurrentStage)
                    .FirstOrDefaultAsync(p => p.Id == ev.ProcedureInstanceId);

                if (procedureInstance == null)
                {
                    _logger.LogWarning("Procedure instance {ProcedureId} not found", ev.ProcedureInstanceId);
                    return;
                }

                // If there's a next stage, update the current stage
                if (ev.NextStageId.HasValue)
                {
                    procedureInstance.CurrentStageId = ev.NextStageId;
                    procedureInstance.UpdatedAt = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    // No next stage means the procedure is completed
                    procedureInstance.Status = "Completed";
                    procedureInstance.CurrentStageId = null;
                    procedureInstance.UpdatedAt = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ProcedureStageTransitionEvent with ID {EventId}", integrationEventId);
                throw;
            }
        }

        private async Task CheckAndUpdateContestStatusAsync(Guid contestInstanceId)
        {
            try
            {
                var contestInstance = await _dbContext.ContestInstances
                    .Include(c => c.ApplicationInstances)
                    .FirstOrDefaultAsync(c => c.Id == contestInstanceId);

                if (contestInstance == null || 
                    contestInstance.Status == "Completed" || 
                    contestInstance.Status == "Interrupted")
                {
                    return;
                }

                // Check if all applications are in a final state (Approved, Rejected, Failed, Interrupted)
                var allApplicationsCompleted = !contestInstance.ApplicationInstances.Any() || 
                    contestInstance.ApplicationInstances.All(a => 
                        a.Status == "Approved" || 
                        a.Status == "Rejected" || 
                        a.Status == "Failed" || 
                        a.Status == "Interrupted");

                if (allApplicationsCompleted)
                {
                    // Get valid statuses from the contest template
                    var template = await _dbContext.ContestTemplates
                        .Where(ct => ct.Id == contestInstance.ContestTemplateId)
                        .FirstOrDefaultAsync();

                    if (template != null && template.StatusModelArray != null && 
                        template.StatusModelArray.Contains("Completed"))
                    {
                        contestInstance.Status = "Completed";
                        contestInstance.EndDate = DateTime.UtcNow;
                        await _dbContext.SaveChangesAsync();

                        // Check if all contests in the procedure are completed or interrupted
                        await CheckAndUpdateProcedureStatusAsync(contestInstance.ProcedureInstanceId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and updating contest status for contest {ContestId}", contestInstanceId);
                throw;
            }
        }

        private async Task CheckAndUpdateProcedureStatusAsync(Guid procedureInstanceId)
        {
            try
            {
                var procedureInstance = await _dbContext.ProcedureInstances
                    .Include(p => p.CurrentStage)
                    .Include(p => p.ContestInstances)
                    .FirstOrDefaultAsync(p => p.Id == procedureInstanceId);

                if (procedureInstance == null || 
                    procedureInstance.Status != "Active" || 
                    procedureInstance.CurrentStage?.StageType != Domain.Enums.ProcedureStageType.Contests)
                {
                    return;
                }

                // Check if all contests are completed or interrupted
                var allContestsFinished = !procedureInstance.ContestInstances.Any() || 
                    procedureInstance.ContestInstances.All(c => 
                        c.Status == "Completed" || 
                        c.Status == "Interrupted");

                if (allContestsFinished)
                {
                    // Get the current stage to find the next stage
                    var currentStage = await _dbContext.ProcedureStageTemplates
                        .FirstOrDefaultAsync(s => s.Id == procedureInstance.CurrentStageId);

                    if (currentStage?.NextStageId != null)
                    {
                        // Publish event to transition to the next stage
                        await _rabbitMQService.PublishEventAsync(new ProcedureStageTransitionEvent
                        {
                            ProcedureInstanceId = procedureInstance.Id,
                            CurrentStageId = currentStage.Id,
                            NextStageId = currentStage.NextStageId
                        });
                    }
                    else
                    {
                        // This was the last stage, mark the procedure as completed
                        procedureInstance.Status = "Completed";
                        procedureInstance.CurrentStageId = null;
                        procedureInstance.UpdatedAt = DateTime.UtcNow;
                        await _dbContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and updating procedure status for procedure {ProcedureId}", procedureInstanceId);
                throw;
            }
        }
    }
}

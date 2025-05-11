using System;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using OrchestratorApp.Domain.Messaging;
using OrchestratorApp.Services;

namespace OrchestratorApp.Services
{
    public class RabbitMQService
    {
        private readonly IBus _bus;
        private readonly ILogger<RabbitMQService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public RabbitMQService(
            IBus bus,
            ILogger<RabbitMQService> logger,
            IServiceProvider serviceProvider)
        {
            _bus = bus;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task PublishEventAsync<T>(T message) where T : IRabbitMessage
        {
            try
            {
                // Check if RabbitMQ is configured
                if (_bus == null)
                {
                    _logger.LogWarning("RabbitMQ is not configured. Message of type {MessageType} not published.", typeof(T).Name);
                    return;
                }
                
                var rabbitMessage = new RabbitMessage<T>
                {
                    IntegrationEventId = Guid.NewGuid(),
                    Data = message
                };

                var topic = typeof(T).Name;
                await _bus.PubSub.PublishAsync(rabbitMessage, topic);
                
                _logger.LogInformation("Published {MessageType} with ID {EventId}", typeof(T).Name, rabbitMessage.IntegrationEventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing {MessageType} to RabbitMQ", typeof(T).Name);
                throw;
            }
        }

        public void ConfigureSubscriptions()
        {
            try
            {
                // Check if RabbitMQ is configured
                if (_bus == null)
                {
                    _logger.LogWarning("RabbitMQ is not configured. Skipping subscriptions setup.");
                    return;
                }
                
                // Get orchestration service for handling events
                var orchestrationService = _serviceProvider.GetService(typeof(OrchestrationService)) as OrchestrationService;
                if (orchestrationService == null)
                {
                    _logger.LogError("Failed to resolve OrchestrationService for RabbitMQ subscriptions");
                    return;
                }

                // Subscribe to CreateApplicationEvent
                _bus.PubSub.Subscribe<RabbitMessage<CreateApplicationEvent>>(
                    "orchestrator.create-application",
                    async msg => await orchestrationService.HandleCreateApplicationEventAsync(msg.Data, msg.IntegrationEventId));

                // Subscribe to StageCompletedEvent
                _bus.PubSub.Subscribe<RabbitMessage<StageCompletedEvent>>(
                    "orchestrator.stage-completed",
                    async msg => await orchestrationService.HandleStageCompletedEventAsync(msg.Data, msg.IntegrationEventId));

                // Subscribe to InterruptContestEvent
                _bus.PubSub.Subscribe<RabbitMessage<InterruptContestEvent>>(
                    "orchestrator.interrupt-contest",
                    async msg => await orchestrationService.HandleInterruptContestEventAsync(msg.Data, msg.IntegrationEventId));

                // Subscribe to ContestTemplateUpdatedEvent
                _bus.PubSub.Subscribe<RabbitMessage<ContestTemplateUpdatedEvent>>(
                    "orchestrator.contest-template-updated",
                    async msg => await orchestrationService.HandleContestTemplateUpdatedEventAsync(msg.Data, msg.IntegrationEventId));

                // Subscribe to ProcedureStageTransitionEvent
                _bus.PubSub.Subscribe<RabbitMessage<ProcedureStageTransitionEvent>>(
                    "orchestrator.procedure-stage-transition",
                    async msg => await orchestrationService.HandleProcedureStageTransitionEventAsync(msg.Data, msg.IntegrationEventId));

                _logger.LogInformation("Configured RabbitMQ subscriptions successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring RabbitMQ subscriptions");
                throw;
            }
        }
    }
}

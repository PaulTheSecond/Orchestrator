using System;

namespace OrchestratorApp.Domain.Messaging
{
    public class RabbitMessage<T> where T : IRabbitMessage
    {
        public Guid IntegrationEventId { get; set; }
        public T Data { get; set; } = default!;
    }
}

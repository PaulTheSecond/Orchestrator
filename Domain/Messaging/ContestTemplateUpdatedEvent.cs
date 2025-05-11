using System;

namespace OrchestratorApp.Domain.Messaging
{
    public class ContestTemplateUpdatedEvent : IRabbitMessage
    {
        public Guid ContestTemplateId { get; set; }
        public int NewVersion { get; set; }
    }
}

using System;

namespace OrchestratorApp.Domain.Messaging
{
    public class InterruptContestEvent : IRabbitMessage
    {
        public Guid ContestInstanceId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}

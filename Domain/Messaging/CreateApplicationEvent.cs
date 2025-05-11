using System;

namespace OrchestratorApp.Domain.Messaging
{
    public class CreateApplicationEvent : IRabbitMessage
    {
        public Guid ContestInstanceId { get; set; }
        public Guid? ExternalApplicationId { get; set; }
        public string ApplicationData { get; set; } = string.Empty;
    }
}

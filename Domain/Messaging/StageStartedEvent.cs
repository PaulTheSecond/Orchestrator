using System;

namespace OrchestratorApp.Domain.Messaging
{
    public class StageStartedEvent : IRabbitMessage
    {
        public Guid ProcedureInstanceId { get; set; }
        public Guid? ContestInstanceId { get; set; }
        public Guid? ApplicationInstanceId { get; set; }
        public Guid StageId { get; set; }
        public string StageType { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
    }
}

using System;

namespace OrchestratorApp.Domain.Messaging
{
    public class ProcedureStageTransitionEvent : IRabbitMessage
    {
        public Guid ProcedureInstanceId { get; set; }
        public Guid CurrentStageId { get; set; }
        public Guid? NextStageId { get; set; }
    }
}

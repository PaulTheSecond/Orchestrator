using System;
using OrchestratorApp.Domain.Enums;

namespace OrchestratorApp.Domain.Messaging
{
    public class StageCompletedEvent : IRabbitMessage
    {
        public Guid ApplicationInstanceId { get; set; }
        public Guid StageId { get; set; }
        public ResultStatus ResultStatus { get; set; }
        public string? ResultData { get; set; }
    }
}

using System;
using OrchestratorApp.Domain.Enums;

namespace OrchestratorApp.Domain.Entities
{
    public class StageResult
    {
        public Guid Id { get; set; }
        public Guid ApplicationInstanceId { get; set; }
        public Guid StageTemplateId { get; set; }
        public ResultStatus ResultStatus { get; set; }
        public string? ResultData { get; set; }
        public DateTime CompletedAt { get; set; }
        public Guid IntegrationEventId { get; set; }
        
        public virtual ApplicationInstance ApplicationInstance { get; set; } = null!;
        public virtual StageTemplate StageTemplate { get; set; } = null!;
    }
}

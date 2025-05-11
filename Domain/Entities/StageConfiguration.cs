using System;

namespace OrchestratorApp.Domain.Entities
{
    public class StageConfiguration
    {
        public Guid Id { get; set; }
        public Guid ContestInstanceId { get; set; }
        public Guid StageTemplateId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        
        public virtual ContestInstance ContestInstance { get; set; } = null!;
        public virtual StageTemplate StageTemplate { get; set; } = null!;
    }
}

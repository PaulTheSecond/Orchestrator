using System;
using OrchestratorApp.Domain.Enums;

namespace OrchestratorApp.Domain.Entities
{
    public class StageTemplate
    {
        public Guid Id { get; set; }
        public Guid ContestTemplateId { get; set; }
        public ContestStageType StageType { get; set; }
        public int Order { get; set; }
        public Guid? PreviousStageId { get; set; }
        public Guid? NextStageId { get; set; }
        public string DefaultServiceName { get; set; } = string.Empty;
        
        public virtual ContestTemplate ContestTemplate { get; set; } = null!;
        public virtual StageTemplate? PreviousStage { get; set; }
        public virtual StageTemplate? NextStage { get; set; }
        public virtual ICollection<StageConfiguration> StageConfigurations { get; set; } = new List<StageConfiguration>();
        public virtual ICollection<StageResult> StageResults { get; set; } = new List<StageResult>();
    }
}

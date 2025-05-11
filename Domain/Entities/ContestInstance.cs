using System;
using System.Collections.Generic;

namespace OrchestratorApp.Domain.Entities
{
    public class ContestInstance
    {
        public Guid Id { get; set; }
        public Guid ProcedureInstanceId { get; set; }
        public Guid ContestTemplateId { get; set; }
        public int ContestTemplateVersion { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? CurrentStageId { get; set; }
        public string Status { get; set; } = "NotStarted";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        public virtual ProcedureInstance ProcedureInstance { get; set; } = null!;
        public virtual ContestTemplate ContestTemplate { get; set; } = null!;
        public virtual StageTemplate? CurrentStage { get; set; }
        public virtual ICollection<StageConfiguration> StageConfigurations { get; set; } = new List<StageConfiguration>();
        public virtual ICollection<ApplicationInstance> ApplicationInstances { get; set; } = new List<ApplicationInstance>();
    }
}

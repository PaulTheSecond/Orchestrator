using System;
using System.Collections.Generic;

namespace OrchestratorApp.Domain.Entities
{
    public class ProcedureInstance
    {
        public Guid Id { get; set; }
        public Guid ProcedureTemplateId { get; set; }
        public int TemplateVersion { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? CurrentStageId { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public virtual ProcedureTemplate ProcedureTemplate { get; set; } = null!;
        public virtual ProcedureStageTemplate? CurrentStage { get; set; }
        public virtual ICollection<ContestInstance> ContestInstances { get; set; } = new List<ContestInstance>();
    }
}

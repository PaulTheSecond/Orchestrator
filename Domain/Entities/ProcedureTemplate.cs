using System;
using System.Collections.Generic;

namespace OrchestratorApp.Domain.Entities
{
    public class ProcedureTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Version { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public virtual ICollection<ProcedureStageTemplate> ProcedureStages { get; set; } = new List<ProcedureStageTemplate>();
        public virtual ICollection<ContestTemplate> ContestTemplates { get; set; } = new List<ContestTemplate>();
        public virtual ICollection<ProcedureInstance> ProcedureInstances { get; set; } = new List<ProcedureInstance>();
    }
}

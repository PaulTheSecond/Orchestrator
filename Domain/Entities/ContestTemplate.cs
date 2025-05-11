using System;
using System.Collections.Generic;

namespace OrchestratorApp.Domain.Entities
{
    public class ContestTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Version { get; set; }
        public bool IsPublished { get; set; }
        public Guid ProcedureTemplateId { get; set; }

        // Используем встроенную поддержку JSON в PostgreSQL
        public string[] StatusModelArray { get; set; } = Array.Empty<string>();

        public virtual ProcedureTemplate ProcedureTemplate { get; set; } = null!;
        public virtual ICollection<StageTemplate> Stages { get; set; } = new List<StageTemplate>();
        public virtual ICollection<ContestInstance> ContestInstances { get; set; } = new List<ContestInstance>();
    }
}
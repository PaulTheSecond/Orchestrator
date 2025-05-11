using System;
using System.Collections.Generic;

namespace OrchestratorApp.Domain.Events
{
    public class ContestTemplateCreatedEvent
    {
        public Guid TemplateId { get; set; }
        public Guid ProcedureTemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Version { get; set; }
        public List<string> StageTypes { get; set; } = new List<string>();
    }
}
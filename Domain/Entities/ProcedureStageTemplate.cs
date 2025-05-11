using System;
using OrchestratorApp.Domain.Enums;

namespace OrchestratorApp.Domain.Entities
{
    public class ProcedureStageTemplate
    {
        public Guid Id { get; set; }
        public Guid ProcedureTemplateId { get; set; }
        public ProcedureStageType StageType { get; set; }
        public int Order { get; set; }
        public Guid? PreviousStageId { get; set; }
        public Guid? NextStageId { get; set; }
        public string? DefaultServiceName { get; set; }
        
        public virtual ProcedureTemplate ProcedureTemplate { get; set; } = null!;
        public virtual ProcedureStageTemplate? PreviousStage { get; set; }
        public virtual ProcedureStageTemplate? NextStage { get; set; }
    }
}

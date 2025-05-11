using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OrchestratorApp.Domain.Enums;

namespace OrchestratorApp.DTOs
{
    public class ProcedureTemplateDTO
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Version { get; set; }
        
        public bool IsPublished { get; set; }
        
        [Required]
        public List<ProcedureStageTemplateDTO> ProcedureStages { get; set; } = new List<ProcedureStageTemplateDTO>();
        
        public List<ContestTemplateDTO>? ContestTemplates { get; set; }
    }

    public class ProcedureStageTemplateDTO
    {
        public Guid Id { get; set; }
        
        [Required]
        [EnumDataType(typeof(ProcedureStageType))]
        public ProcedureStageType StageType { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Order { get; set; }
        
        public Guid? PreviousStageId { get; set; }
        
        public Guid? NextStageId { get; set; }
        
        [StringLength(255)]
        public string? DefaultServiceName { get; set; }
    }

    public class PublishProcedureTemplateDTO
    {
        [Required]
        public Guid Id { get; set; }
    }
}

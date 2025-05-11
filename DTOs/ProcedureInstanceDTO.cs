using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchestratorApp.DTOs
{
    public class ProcedureInstanceDTO
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid ProcedureTemplateId { get; set; }
        
        [Required]
        public int TemplateVersion { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;
        
        public Guid? CurrentStageId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active";
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        public List<ContestInstanceDTO>? ContestInstances { get; set; }
    }

    public class CreateProcedureInstanceDTO
    {
        [Required]
        public Guid ProcedureTemplateId { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;
    }

    public class TransitionProcedureStageDTO
    {
        [Required]
        public Guid ProcedureInstanceId { get; set; }
    }
}

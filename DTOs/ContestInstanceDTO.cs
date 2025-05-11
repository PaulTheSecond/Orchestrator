using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchestratorApp.DTOs
{
    public class ContestInstanceDTO
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid ProcedureInstanceId { get; set; }
        
        [Required]
        public Guid ContestTemplateId { get; set; }
        
        [Required]
        public int ContestTemplateVersion { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;
        
        public Guid? CurrentStageId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "NotStarted";
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public List<StageConfigurationDTO>? StageConfigurations { get; set; }
        
        public List<ApplicationInstanceDTO>? ApplicationInstances { get; set; }
    }

    public class CreateContestInstanceDTO
    {
        [Required]
        public Guid ProcedureInstanceId { get; set; }
        
        [Required]
        public Guid ContestTemplateId { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public List<StageConfigurationDTO> StageConfigurations { get; set; } = new List<StageConfigurationDTO>();
    }

    public class InterruptContestDTO
    {
        [Required]
        public Guid ContestInstanceId { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}

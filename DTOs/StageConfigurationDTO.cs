using System;
using System.ComponentModel.DataAnnotations;

namespace OrchestratorApp.DTOs
{
    public class StageConfigurationDTO
    {
        public Guid Id { get; set; }
        
        public Guid ContestInstanceId { get; set; }
        
        [Required]
        public Guid StageTemplateId { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        [Required]
        [StringLength(255)]
        public string ServiceName { get; set; } = string.Empty;
    }
}

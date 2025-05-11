using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchestratorApp.DTOs
{
    public class ApplicationInstanceDTO
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid ContestInstanceId { get; set; }
        
        public Guid? CurrentStageId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Submitted";
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        
        public Guid? ExternalApplicationId { get; set; }
        
        public List<StageResultDTO>? StageResults { get; set; }
    }

    public class CreateApplicationDTO
    {
        [Required]
        public Guid ContestInstanceId { get; set; }
        
        public Guid? ExternalApplicationId { get; set; }
        
        public string ApplicationData { get; set; } = string.Empty;
    }

    public class StageResultDTO
    {
        public Guid Id { get; set; }
        
        public Guid ApplicationInstanceId { get; set; }
        
        public Guid StageTemplateId { get; set; }
        
        [Required]
        [EnumDataType(typeof(Domain.Enums.ResultStatus))]
        public Domain.Enums.ResultStatus ResultStatus { get; set; }
        
        public string? ResultData { get; set; }
        
        public DateTime CompletedAt { get; set; }
        
        public Guid IntegrationEventId { get; set; }
    }
}

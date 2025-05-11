using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OrchestratorApp.Domain.Enums;

namespace OrchestratorApp.DTOs
{
    public class ContestTemplateDTO
    {
        public Guid Id { get; set; }

        [Required]
        public Guid ProcedureTemplateId { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int Version { get; set; }

        public bool IsPublished { get; set; }

        [Required]
        public string[] StatusModelArray { get; set; } = Array.Empty<string>();

        [Required]
        public List<StageTemplateDTO> Stages { get; set; } = new List<StageTemplateDTO>();
    }

    public class StageTemplateDTO
    {
        public Guid Id { get; set; }

        [Required]
        [EnumDataType(typeof(ContestStageType))]
        public ContestStageType StageType { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Order { get; set; }

        public Guid? PreviousStageId { get; set; }

        public Guid? NextStageId { get; set; }

        [Required]
        [StringLength(255)]
        public string DefaultServiceName { get; set; } = string.Empty;
    }

    public class CreateContestTemplateDTO
    {
        [Required]
        public Guid ProcedureTemplateId { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string[] StatusModelArray { get; set; } = Array.Empty<string>();

        [Required]
        [MinLength(1)]
        public List<CreateStageTemplateDTO> Stages { get; set; } = new List<CreateStageTemplateDTO>();
    }

    public class CreateStageTemplateDTO
    {
        [Required]
        [EnumDataType(typeof(ContestStageType))]
        public ContestStageType StageType { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Order { get; set; }

        [Required]
        [StringLength(255)]
        public string DefaultServiceName { get; set; } = string.Empty;
    }

    public class UpdateContestTemplateDTO
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string[] StatusModelArray { get; set; } = Array.Empty<string>();

        [Required]
        [MinLength(1)]
        public List<CreateStageTemplateDTO> Stages { get; set; } = new List<CreateStageTemplateDTO>();
    }

    public class UpdateStageTemplateDTO
    {
        public Guid Id { get; set; }

        [Required]
        [EnumDataType(typeof(ContestStageType))]
        public ContestStageType StageType { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Order { get; set; }

        public Guid? PreviousStageId { get; set; }

        public Guid? NextStageId { get; set; }

        [Required]
        [StringLength(255)]
        public string DefaultServiceName { get; set; } = string.Empty;
    }

    public class PublishContestTemplateDTO
    {
        [Required]
        public Guid Id { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace OrchestratorApp.Domain.Entities
{
    public class ApplicationInstance
    {
        public Guid Id { get; set; }
        public Guid ContestInstanceId { get; set; }
        public Guid? CurrentStageId { get; set; }
        public string Status { get; set; } = "Submitted";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? ExternalApplicationId { get; set; }
        
        public virtual ContestInstance ContestInstance { get; set; } = null!;
        public virtual StageTemplate? CurrentStage { get; set; }
        public virtual ICollection<StageResult> StageResults { get; set; } = new List<StageResult>();
    }
}

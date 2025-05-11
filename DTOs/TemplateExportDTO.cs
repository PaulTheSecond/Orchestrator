using System;
using System.Collections.Generic;

namespace OrchestratorApp.DTOs
{
    /// <summary>
    /// DTO для экспорта/импорта полного шаблона процедуры с зависимыми объектами
    /// </summary>
    public class TemplateExportDTO
    {
        // Метаданные экспорта
        public string ExportVersion { get; set; } = "1.0";
        public DateTime ExportDate { get; set; } = DateTime.UtcNow;
        
        // Основные данные
        public ProcedureTemplateDTO ProcedureTemplate { get; set; } = null!;
        
        // Вспомогательные поля
        public string Name => ProcedureTemplate?.Name ?? "Unnamed Template";
        public int Version => ProcedureTemplate?.Version ?? 1;
        public bool IsPublished => ProcedureTemplate?.IsPublished ?? false;
    }
}
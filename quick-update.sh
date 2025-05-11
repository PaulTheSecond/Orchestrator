#!/bin/bash
echo "Creating API controllers for template export..."

# Создаем контроллер API для экспорта шаблонов если его нет
if [ ! -f "/home/runner/workspace/Controllers/TemplateExportController.cs" ]; then
  cat > "/home/runner/workspace/Controllers/TemplateExportController.cs" << 'EOF'
using Microsoft.AspNetCore.Mvc;
using OrchestratorApp.DTOs;
using OrchestratorApp.Services;
using System;
using System.Text;
using System.Threading.Tasks;

namespace OrchestratorApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TemplateExportController : ControllerBase
    {
        private readonly ProcedureTemplateService _procedureTemplateService;

        public TemplateExportController(ProcedureTemplateService procedureTemplateService)
        {
            _procedureTemplateService = procedureTemplateService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ExportTemplate(Guid id)
        {
            try
            {
                var template = await _procedureTemplateService.ExportTemplate(id);
                if (template == null)
                {
                    return NotFound("Template not found");
                }

                var jsonContent = System.Text.Json.JsonSerializer.Serialize(template);
                var bytes = Encoding.UTF8.GetBytes(jsonContent);
                
                return File(bytes, "application/json", $"template_{id}.json");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProcedureTemplateDTO>> ImportTemplate()
        {
            try
            {
                if (Request.Form.Files.Count == 0)
                {
                    return BadRequest(new { error = "No file uploaded" });
                }

                var file = Request.Form.Files[0];
                if (file.Length == 0)
                {
                    return BadRequest(new { error = "Empty file" });
                }

                // Читаем содержимое файла
                using var reader = new System.IO.StreamReader(file.OpenReadStream());
                var jsonContent = await reader.ReadToEndAsync();

                // Импортируем шаблон
                var importedTemplate = await _procedureTemplateService.ImportTemplate(jsonContent);
                if (importedTemplate == null)
                {
                    return BadRequest(new { error = "Failed to import template" });
                }

                return Ok(importedTemplate);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
EOF
  echo "Created TemplateExportController.cs"
fi

# Создаем DTO для экспорта/импорта шаблонов если его нет
if [ ! -f "/home/runner/workspace/DTOs/TemplateExportDTO.cs" ]; then
  cat > "/home/runner/workspace/DTOs/TemplateExportDTO.cs" << 'EOF'
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OrchestratorApp.DTOs
{
    public class TemplateExportDTO
    {
        [JsonPropertyName("procedureTemplate")]
        public ProcedureTemplateDTO ProcedureTemplate { get; set; }

        [JsonPropertyName("contestTemplates")]
        public List<ContestTemplateDTO> ContestTemplates { get; set; } = new List<ContestTemplateDTO>();
    }
}
EOF
  echo "Created TemplateExportDTO.cs"
fi

echo "Done! Now modify the Angular components to use Bootstrap instead of Material."
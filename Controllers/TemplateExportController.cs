using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchestratorApp.DTOs;
using OrchestratorApp.Services;

namespace OrchestratorApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TemplateExportController : ControllerBase
    {
        private readonly ProcedureTemplateService _procedureTemplateService;
        private readonly ILogger<TemplateExportController> _logger;
        
        // Параметры сериализации JSON
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        public TemplateExportController(
            ProcedureTemplateService procedureTemplateService,
            ILogger<TemplateExportController> logger)
        {
            _procedureTemplateService = procedureTemplateService;
            _logger = logger;
        }
        
        /// <summary>
        /// Экспортирует шаблон процедуры в файл JSON
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ExportTemplate(Guid id)
        {
            try
            {
                // Получаем данные для экспорта
                var exportData = await _procedureTemplateService.ExportProcedureTemplateAsync(id);
                
                // Сериализуем в JSON
                var json = JsonSerializer.Serialize(exportData, _jsonOptions);
                
                // Генерируем имя файла на основе имени шаблона и даты
                var fileName = $"{exportData.Name.Replace(" ", "_")}_v{exportData.Version}_{DateTime.UtcNow:yyyy-MM-dd}.json";
                
                // Возвращаем файл
                var bytes = Encoding.UTF8.GetBytes(json);
                return File(bytes, "application/json", fileName);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Template not found");
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting template");
                return StatusCode(500, new { Error = "Failed to export template", Message = ex.Message });
            }
        }
        
        /// <summary>
        /// Импортирует шаблон процедуры из файла JSON
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ImportTemplate(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }
                
                // Считываем содержимое файла
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                
                // Десериализуем данные
                var importData = JsonSerializer.Deserialize<TemplateExportDTO>(json, _jsonOptions);
                
                if (importData == null)
                {
                    return BadRequest("Invalid template data format");
                }
                
                // Импортируем шаблон
                var result = await _procedureTemplateService.ImportProcedureTemplateAsync(importData);
                
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid import data");
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing template");
                return StatusCode(500, new { Error = "Failed to import template", Message = ex.Message });
            }
        }
    }
}
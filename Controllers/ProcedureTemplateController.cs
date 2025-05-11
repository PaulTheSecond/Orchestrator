using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchestratorApp.DTOs;
using OrchestratorApp.Services;

namespace OrchestratorApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProcedureTemplateController : ControllerBase
    {
        private readonly ProcedureTemplateService _procedureTemplateService;
        private readonly ILogger<ProcedureTemplateController> _logger;

        public ProcedureTemplateController(
            ProcedureTemplateService procedureTemplateService,
            ILogger<ProcedureTemplateController> logger)
        {
            _procedureTemplateService = procedureTemplateService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProcedureTemplateDTO>>> GetAll()
        {
            try
            {
                var templates = await _procedureTemplateService.GetAllProcedureTemplatesAsync();
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving procedure templates");
                return StatusCode(500, new { error = "An error occurred while retrieving procedure templates", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProcedureTemplateDTO>> Get(Guid id)
        {
            try
            {
                var template = await _procedureTemplateService.GetProcedureTemplateAsync(id);
                if (template == null)
                {
                    return NotFound(new { error = $"Procedure template with ID {id} not found" });
                }
                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving procedure template");
                return StatusCode(500, new { error = "An error occurred while retrieving the procedure template", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProcedureTemplateDTO>> Create([FromBody] ProcedureTemplateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var template = await _procedureTemplateService.CreateProcedureTemplateAsync(dto);
                return CreatedAtAction(nameof(Get), new { id = template.Id }, template);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid procedure template data");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating procedure template");
                return StatusCode(500, new { error = "An error occurred while creating the procedure template", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProcedureTemplateDTO>> Update(Guid id, [FromBody] ProcedureTemplateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != dto.Id)
                {
                    return BadRequest(new { error = "ID in the URL does not match the ID in the request body" });
                }

                var template = await _procedureTemplateService.UpdateProcedureTemplateAsync(id, dto);
                return Ok(template);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Procedure template not found");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation on procedure template");
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid procedure template data");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating procedure template");
                return StatusCode(500, new { error = "An error occurred while updating the procedure template", details = ex.Message });
            }
        }

        [HttpPost("{id}/publish")]
        public async Task<ActionResult<ProcedureTemplateDTO>> Publish(Guid id)
        {
            try
            {
                var template = await _procedureTemplateService.PublishProcedureTemplateAsync(id);
                return Ok(template);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Procedure template not found");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation on procedure template");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing procedure template");
                return StatusCode(500, new { error = "An error occurred while publishing the procedure template", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                await _procedureTemplateService.DeleteProcedureTemplateAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Procedure template not found");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation on procedure template");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting procedure template");
                return StatusCode(500, new { error = "An error occurred while deleting the procedure template", details = ex.Message });
            }
        }
    }
}

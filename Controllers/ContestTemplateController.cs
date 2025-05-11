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
    public class ContestTemplateController : ControllerBase
    {
        private readonly ContestTemplateService _contestTemplateService;
        private readonly ILogger<ContestTemplateController> _logger;

        public ContestTemplateController(
            ContestTemplateService contestTemplateService,
            ILogger<ContestTemplateController> logger)
        {
            _contestTemplateService = contestTemplateService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContestTemplateDTO>>> GetAll()
        {
            try
            {
                var templates = await _contestTemplateService.GetAllContestTemplatesAsync();
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contest templates");
                return StatusCode(500, new { error = "An error occurred while retrieving contest templates", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ContestTemplateDTO>> Get(Guid id)
        {
            try
            {
                var template = await _contestTemplateService.GetContestTemplateAsync(id);
                if (template == null)
                {
                    return NotFound(new { error = $"Contest template with ID {id} not found" });
                }
                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contest template");
                return StatusCode(500, new { error = "An error occurred while retrieving the contest template", details = ex.Message });
            }
        }

        [HttpGet("byProcedure/{procedureTemplateId}")]
        public async Task<ActionResult<IEnumerable<ContestTemplateDTO>>> GetByProcedure(Guid procedureTemplateId)
        {
            try
            {
                var templates = await _contestTemplateService.GetContestTemplatesByProcedureAsync(procedureTemplateId);
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contest templates by procedure");
                return StatusCode(500, new { error = "An error occurred while retrieving contest templates", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ContestTemplateDTO>> Create([FromBody] CreateContestTemplateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var template = await _contestTemplateService.CreateContestTemplateAsync(dto);
                return CreatedAtAction(nameof(Get), new { id = template.Id }, template);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid contest template data");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contest template");
                return StatusCode(500, new { error = "An error occurred while creating the contest template", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ContestTemplateDTO>> Update(Guid id, [FromBody] UpdateContestTemplateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var template = await _contestTemplateService.UpdateContestTemplateAsync(id, dto);
                return Ok(template);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Contest template not found");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation on contest template");
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid contest template data");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contest template");
                return StatusCode(500, new { error = "An error occurred while updating the contest template", details = ex.Message });
            }
        }

        [HttpPost("{id}/publish")]
        public async Task<ActionResult<ContestTemplateDTO>> Publish(Guid id)
        {
            try
            {
                var template = await _contestTemplateService.PublishContestTemplateAsync(id);
                return Ok(template);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Contest template not found");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation on contest template");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing contest template");
                return StatusCode(500, new { error = "An error occurred while publishing the contest template", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                await _contestTemplateService.DeleteContestTemplateAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Contest template not found");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation on contest template");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contest template");
                return StatusCode(500, new { error = "An error occurred while deleting the contest template", details = ex.Message });
            }
        }
    }
}

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
    public class ContestInstanceController : ControllerBase
    {
        private readonly ContestInstanceService _contestInstanceService;
        private readonly ILogger<ContestInstanceController> _logger;

        public ContestInstanceController(
            ContestInstanceService contestInstanceService,
            ILogger<ContestInstanceController> logger)
        {
            _contestInstanceService = contestInstanceService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContestInstanceDTO>>> GetAll()
        {
            try
            {
                var instances = await _contestInstanceService.GetAllContestInstancesAsync();
                return Ok(instances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contest instances");
                return StatusCode(500, new { error = "An error occurred while retrieving contest instances", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ContestInstanceDTO>> Get(Guid id)
        {
            try
            {
                var instance = await _contestInstanceService.GetContestInstanceAsync(id);
                if (instance == null)
                {
                    return NotFound(new { error = $"Contest instance with ID {id} not found" });
                }
                return Ok(instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contest instance");
                return StatusCode(500, new { error = "An error occurred while retrieving the contest instance", details = ex.Message });
            }
        }

        [HttpGet("byProcedure/{procedureInstanceId}")]
        public async Task<ActionResult<IEnumerable<ContestInstanceDTO>>> GetByProcedure(Guid procedureInstanceId)
        {
            try
            {
                var instances = await _contestInstanceService.GetContestInstancesByProcedureAsync(procedureInstanceId);
                return Ok(instances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contest instances by procedure");
                return StatusCode(500, new { error = "An error occurred while retrieving contest instances", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ContestInstanceDTO>> Create([FromBody] CreateContestInstanceDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var instance = await _contestInstanceService.CreateContestInstanceAsync(dto);
                return CreatedAtAction(nameof(Get), new { id = instance.Id }, instance);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid contest instance data");
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for contest instance");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contest instance");
                return StatusCode(500, new { error = "An error occurred while creating the contest instance", details = ex.Message });
            }
        }

        [HttpPost("{id}/interrupt")]
        public async Task<ActionResult<ContestInstanceDTO>> Interrupt(Guid id, [FromBody] InterruptContestDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != dto.ContestInstanceId)
                {
                    return BadRequest(new { error = "ID in the URL does not match the ID in the request body" });
                }

                var instance = await _contestInstanceService.InterruptContestAsync(dto);
                return Ok(instance);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Contest instance not found");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation on contest instance");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interrupting contest instance");
                return StatusCode(500, new { error = "An error occurred while interrupting the contest instance", details = ex.Message });
            }
        }

        [HttpPost("{id}/updateTemplate")]
        public async Task<ActionResult<ContestInstanceDTO>> UpdateTemplateVersion(Guid id, [FromBody] int newVersion)
        {
            try
            {
                var instance = await _contestInstanceService.UpdateContestTemplateVersionAsync(id, newVersion);
                return Ok(instance);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Contest instance not found");
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid contest template version");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contest template version");
                return StatusCode(500, new { error = "An error occurred while updating the contest template version", details = ex.Message });
            }
        }
    }
}

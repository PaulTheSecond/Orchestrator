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
    public class ProcedureInstanceController : ControllerBase
    {
        private readonly ProcedureInstanceService _procedureInstanceService;
        private readonly ILogger<ProcedureInstanceController> _logger;

        public ProcedureInstanceController(
            ProcedureInstanceService procedureInstanceService,
            ILogger<ProcedureInstanceController> logger)
        {
            _procedureInstanceService = procedureInstanceService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProcedureInstanceDTO>>> GetAll()
        {
            try
            {
                var instances = await _procedureInstanceService.GetAllProcedureInstancesAsync();
                return Ok(instances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving procedure instances");
                return StatusCode(500, new { error = "An error occurred while retrieving procedure instances", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProcedureInstanceDTO>> Get(Guid id)
        {
            try
            {
                var instance = await _procedureInstanceService.GetProcedureInstanceAsync(id);
                if (instance == null)
                {
                    return NotFound(new { error = $"Procedure instance with ID {id} not found" });
                }
                return Ok(instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving procedure instance");
                return StatusCode(500, new { error = "An error occurred while retrieving the procedure instance", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProcedureInstanceDTO>> Create([FromBody] CreateProcedureInstanceDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var instance = await _procedureInstanceService.CreateProcedureInstanceAsync(dto);
                return CreatedAtAction(nameof(Get), new { id = instance.Id }, instance);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid procedure instance data");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating procedure instance");
                return StatusCode(500, new { error = "An error occurred while creating the procedure instance", details = ex.Message });
            }
        }

        [HttpPost("{id}/transition")]
        public async Task<ActionResult<ProcedureInstanceDTO>> TransitionToNextStage(Guid id)
        {
            try
            {
                var instance = await _procedureInstanceService.TransitionToNextStageAsync(id);
                return Ok(instance);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Procedure instance not found");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation on procedure instance");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transitioning procedure instance to next stage");
                return StatusCode(500, new { error = "An error occurred while transitioning the procedure stage", details = ex.Message });
            }
        }
    }
}

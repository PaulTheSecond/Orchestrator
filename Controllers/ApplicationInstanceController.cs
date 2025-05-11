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
    public class ApplicationInstanceController : ControllerBase
    {
        private readonly ApplicationInstanceService _applicationInstanceService;
        private readonly ILogger<ApplicationInstanceController> _logger;

        public ApplicationInstanceController(
            ApplicationInstanceService applicationInstanceService,
            ILogger<ApplicationInstanceController> logger)
        {
            _applicationInstanceService = applicationInstanceService;
            _logger = logger;
        }

        [HttpGet("byContest/{contestInstanceId}")]
        public async Task<ActionResult<IEnumerable<ApplicationInstanceDTO>>> GetByContest(Guid contestInstanceId)
        {
            try
            {
                var applications = await _applicationInstanceService.GetApplicationsByContestAsync(contestInstanceId);
                return Ok(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving application instances");
                return StatusCode(500, new { error = "An error occurred while retrieving application instances", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationInstanceDTO>> Get(Guid id)
        {
            try
            {
                var application = await _applicationInstanceService.GetApplicationInstanceAsync(id);
                if (application == null)
                {
                    return NotFound(new { error = $"Application instance with ID {id} not found" });
                }
                return Ok(application);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving application instance");
                return StatusCode(500, new { error = "An error occurred while retrieving the application instance", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApplicationInstanceDTO>> Create([FromBody] CreateApplicationDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var application = await _applicationInstanceService.CreateApplicationInstanceAsync(dto);
                return CreatedAtAction(nameof(Get), new { id = application.Id }, application);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid application instance data");
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for application instance");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating application instance");
                return StatusCode(500, new { error = "An error occurred while creating the application instance", details = ex.Message });
            }
        }
    }
}

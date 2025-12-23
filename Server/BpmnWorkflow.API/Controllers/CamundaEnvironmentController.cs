using BpmnWorkflow.Application.DTOs.Camunda;
using BpmnWorkflow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BpmnWorkflow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CamundaEnvironmentController : ControllerBase
    {
        private readonly ICamundaEnvironmentService _envService;
        private readonly ILogger<CamundaEnvironmentController> _logger;

        public CamundaEnvironmentController(ICamundaEnvironmentService envService, ILogger<CamundaEnvironmentController> logger)
        {
            _envService = envService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<CamundaEnvironmentDto>>> GetAll()
        {
            return Ok(await _envService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CamundaEnvironmentDto>> GetById(Guid id)
        {
            var env = await _envService.GetByIdAsync(id);
            if (env == null) return NotFound();
            return Ok(env);
        }

        [HttpPost]
        public async Task<ActionResult<CamundaEnvironmentDto>> Create(CamundaEnvironmentUpsertDto dto)
        {
            var env = await _envService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = env.Id }, env);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CamundaEnvironmentDto>> Update(Guid id, CamundaEnvironmentUpsertDto dto)
        {
            var env = await _envService.UpdateAsync(id, dto);
            if (env == null) return NotFound();
            return Ok(env);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _envService.DeleteAsync(id);
            if (!success) return BadRequest("Could not delete environment. It might be active or doesn't exist.");
            return NoContent();
        }

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(Guid id)
        {
            var success = await _envService.SetActiveAsync(id);
            if (!success) return NotFound();
            return Ok();
        }

        [HttpGet("active")]
        public async Task<ActionResult<CamundaEnvironmentDto>> GetActive()
        {
            var env = await _envService.GetActiveAsync();
            if (env == null) return NotFound("No active environment found.");
            return Ok(env);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using BpmnWorkflow.Client.Models;
using BpmnWorkflow.Application.Interfaces;
using BpmnWorkflow.Infrastructure.Data;
using BpmnWorkflow.Domain.Entities;

namespace BpmnWorkflow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkflowsController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;
        private readonly IAuditService _auditService;

        public WorkflowsController(IWorkflowService workflowService, IAuditService auditService)
        {
            _workflowService = workflowService;
            _auditService = auditService;
        }

        [HttpGet("{id}/audit")]
        public async Task<IActionResult> GetAuditLogs(Guid id)
        {
            var logs = await _auditService.GetWorkflowAuditLogsAsync(id);
            return Ok(logs);
        }

        [HttpGet]
        public async Task<IActionResult> GetWorkflows([FromQuery] string? search = null)
        {
            var workflows = await _workflowService.GetAllAsync(search);
            return Ok(workflows);
        }

        [HttpGet("test")]
        public IActionResult Test() => Ok("Workflows controller is alive");

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string? search)
        {
            // Handle defaults here if they are not provided
            var pageValue = page <= 0 ? 1 : page;
            var pageSizeValue = pageSize <= 0 ? 10 : pageSize;

            var result = await _workflowService.GetPaginatedAsync(pageValue, pageSizeValue, search);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkflow(Guid id)
        {
            var workflow = await _workflowService.GetByIdAsync(id);
            if (workflow == null) return NotFound();
            return Ok(workflow);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkflow([FromBody] UpsertWorkflowRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                var workflow = await _workflowService.CreateAsync(request, userId);
                return CreatedAtAction(nameof(GetWorkflow), new { id = workflow.Id }, workflow);
            }
            catch (Exception ex) when (ex.InnerException?.Message.Contains("FOREIGN KEY") == true || ex.Message.Contains("FOREIGN KEY") == true)
            {
                return BadRequest("Your user account was not found in the database. Please logout and login again to refresh your session.");
            }
            catch (Exception ex)
            {
                // Rethrow to be caught by global exception middleware or return 500
                return StatusCode(500, new { message = "An error occurred while creating the workflow.", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkflow(Guid id, [FromBody] UpsertWorkflowRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var workflow = await _workflowService.UpdateAsync(id, request, userId);
            if (workflow == null) return NotFound();
            return Ok(workflow);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkflow(Guid id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var success = await _workflowService.DeleteAsync(id, userId);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}/versions")]
        public async Task<IActionResult> GetWorkflowVersions(Guid id)
        {
            var versions = await _workflowService.GetVersionsAsync(id);
            return Ok(versions);
        }

        [HttpPost("{id}/restore/{versionId}")]
        public async Task<IActionResult> RestoreVersion(Guid id, Guid versionId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var workflow = await _workflowService.RestoreVersionAsync(id, versionId, userId);
            if (workflow == null) return NotFound();
            return Ok(workflow);
        }
    }
}

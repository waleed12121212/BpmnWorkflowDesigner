using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BpmnWorkflow.Client.Models;
using BpmnWorkflow.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace BpmnWorkflow.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FormsController : ControllerBase
    {
        private readonly IFormService _formService;

        public FormsController(IFormService formService)
        {
            _formService = formService;
        }

        [HttpGet]
        public async Task<IActionResult> GetForms([FromQuery] string? search)
        {
            var forms = await _formService.GetAllAsync(search);
            return Ok(forms);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetForm(Guid id)
        {
            var form = await _formService.GetByIdAsync(id);
            if (form == null) return NotFound();
            return Ok(form);
        }

        [HttpPost]
        public async Task<IActionResult> CreateForm([FromBody] UpsertFormRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var form = await _formService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetForm), new { id = form.Id }, form);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateForm(Guid id, [FromBody] UpsertFormRequest request, [FromQuery] string? comment)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var form = await _formService.UpdateAsync(id, request, userId, comment);
            if (form == null) return NotFound();
            return Ok(form);
        }

        [HttpGet("{id}/versions")]
        public async Task<IActionResult> GetFormVersions(Guid id)
        {
            var versions = await _formService.GetVersionsAsync(id);
            return Ok(versions);
        }

        [HttpGet("{id}/audit-logs")]
        public async Task<IActionResult> GetFormAuditLogs(Guid id)
        {
            var logs = await _formService.GetAuditLogsAsync(id);
            return Ok(logs);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteForm(Guid id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var result = await _formService.DeleteAsync(id, userId);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}

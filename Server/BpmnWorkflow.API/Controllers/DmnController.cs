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
    public class DmnController : ControllerBase
    {
        private readonly IDmnService _dmnService;

        public DmnController(IDmnService dmnService)
        {
            _dmnService = dmnService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDmns([FromQuery] string? search)
        {
            var dmns = await _dmnService.GetAllAsync(search);
            return Ok(dmns);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDmn(Guid id)
        {
            var dmn = await _dmnService.GetByIdAsync(id);
            if (dmn == null) return NotFound();
            return Ok(dmn);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDmn([FromBody] UpsertDmnRequest request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var dmn = await _dmnService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetDmn), new { id = dmn.Id }, dmn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDmn(Guid id, [FromBody] UpsertDmnRequest request, [FromQuery] string? comment)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var dmn = await _dmnService.UpdateAsync(id, request, userId, comment);
            if (dmn == null) return NotFound();
            return Ok(dmn);
        }

        [HttpGet("{id}/versions")]
        public async Task<IActionResult> GetDmnVersions(Guid id)
        {
            var versions = await _dmnService.GetVersionsAsync(id);
            return Ok(versions);
        }

        [HttpGet("{id}/audit-logs")]
        public async Task<IActionResult> GetDmnAuditLogs(Guid id)
        {
            var logs = await _dmnService.GetAuditLogsAsync(id);
            return Ok(logs);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDmn(Guid id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var result = await _dmnService.DeleteAsync(id, userId);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}

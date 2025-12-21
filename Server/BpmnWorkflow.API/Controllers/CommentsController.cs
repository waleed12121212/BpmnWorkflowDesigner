using BpmnWorkflow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BpmnWorkflow.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("workflow/{workflowId}")]
        public async Task<IActionResult> GetComments(Guid workflowId)
        {
            var comments = await _commentService.GetWorkflowCommentsAsync(workflowId);
            return Ok(comments);
        }

        [HttpPost("workflow/{workflowId}")]
        public async Task<IActionResult> AddComment(Guid workflowId, [FromBody] CommentRequest request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var comment = await _commentService.AddCommentAsync(workflowId, userId, request.Text, request.ElementId);
            return Ok(comment);
        }

        [HttpGet("form/{formId}")]
        public async Task<IActionResult> GetFormComments(Guid formId)
        {
            var comments = await _commentService.GetFormCommentsAsync(formId);
            return Ok(comments);
        }

        [HttpPost("form/{formId}")]
        public async Task<IActionResult> AddFormComment(Guid formId, [FromBody] CommentRequest request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var comment = await _commentService.AddFormCommentAsync(formId, userId, request.Text);
            return Ok(comment);
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var success = await _commentService.DeleteCommentAsync(commentId, userId);
            if (!success) return BadRequest("Could not delete comment");

            return Ok();
        }
    }

    public class CommentRequest
    {
        public string Text { get; set; } = string.Empty;
        public string? ElementId { get; set; }
    }
}

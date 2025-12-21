using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BpmnWorkflow.Client.Models;

namespace BpmnWorkflow.Application.Interfaces
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentDto>> GetWorkflowCommentsAsync(Guid workflowId);
        Task<CommentDto> AddCommentAsync(Guid workflowId, Guid userId, string text, string? elementId = null);
        Task<IEnumerable<CommentDto>> GetFormCommentsAsync(Guid formId);
        Task<CommentDto> AddFormCommentAsync(Guid formId, Guid userId, string text);
        Task<bool> DeleteCommentAsync(Guid commentId, Guid userId);
    }
}

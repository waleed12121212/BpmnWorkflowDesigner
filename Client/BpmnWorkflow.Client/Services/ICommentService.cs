using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BpmnWorkflow.Client.Models;

namespace BpmnWorkflow.Client.Services
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentDto>> GetCommentsAsync(Guid workflowId);
        Task<CommentDto?> AddCommentAsync(Guid workflowId, string text, string? elementId = null);
        Task<IEnumerable<CommentDto>> GetFormCommentsAsync(Guid formId);
        Task<CommentDto?> AddFormCommentAsync(Guid formId, string text);
        Task<bool> DeleteCommentAsync(Guid commentId);
    }
}

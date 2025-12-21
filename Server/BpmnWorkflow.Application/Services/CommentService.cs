using BpmnWorkflow.Application.Interfaces;
using BpmnWorkflow.Client.Models;
using BpmnWorkflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BpmnWorkflow.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly IApplicationDbContext _context;

        public CommentService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CommentDto>> GetWorkflowCommentsAsync(Guid workflowId)
        {
            return await _context.Comments
                .AsNoTracking()
                .Where(c => c.WorkflowId == workflowId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    WorkflowId = c.WorkflowId,
                    UserId = c.UserId,
                    UserName = c.User != null ? c.User.Username : "Unknown",
                    Text = c.Text,
                    CreatedAt = c.CreatedAt,
                    ElementId = c.ElementId
                })
                .ToListAsync();
        }

        public async Task<CommentDto> AddCommentAsync(Guid workflowId, Guid userId, string text, string? elementId = null)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new Exception("User not found");

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflowId,
                UserId = userId,
                Text = text,
                CreatedAt = DateTime.UtcNow,
                ElementId = elementId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return new CommentDto
            {
                Id = comment.Id,
                WorkflowId = comment.WorkflowId,
                UserId = comment.UserId,
                UserName = user.Username,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
                ElementId = comment.ElementId
            };
        }

        public async Task<IEnumerable<CommentDto>> GetFormCommentsAsync(Guid formId)
        {
            return await _context.Comments
                .AsNoTracking()
                .Where(c => c.FormId == formId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    FormId = c.FormId,
                    UserId = c.UserId,
                    UserName = c.User != null ? c.User.Username : "Unknown",
                    Text = c.Text,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<CommentDto> AddFormCommentAsync(Guid formId, Guid userId, string text)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new Exception("User not found");

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                FormId = formId,
                UserId = userId,
                Text = text,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return new CommentDto
            {
                Id = comment.Id,
                FormId = comment.FormId,
                UserId = comment.UserId,
                UserName = user.Username,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) return false;

            // Only allow deletion if the user is the author
            if (comment.UserId != userId) return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

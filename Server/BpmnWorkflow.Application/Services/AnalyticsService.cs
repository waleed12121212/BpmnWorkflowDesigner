using BpmnWorkflow.Application.Interfaces;
using BpmnWorkflow.Client.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BpmnWorkflow.Application.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IApplicationDbContext _context;

        public AnalyticsService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardAnalyticsDto> GetDashboardAnalyticsAsync()
        {
            var analytics = new DashboardAnalyticsDto();

            // Basic Counts
            analytics.TotalWorkflows = await _context.Workflows.CountAsync(w => !w.IsDeleted);
            analytics.TotalUsers = await _context.Users.CountAsync();
            analytics.TotalComments = await _context.Comments.CountAsync();
            analytics.TotalProcessInstances = await _context.ProcessInstances.CountAsync();

            // Workflow Creation Trend (Last 7 days)
            var last7Days = DateTime.UtcNow.AddDays(-6).Date;
            var trendQuery = await _context.Workflows
                .Where(w => !w.IsDeleted && w.CreatedAt >= last7Days)
                .GroupBy(w => w.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var trendData = Enumerable.Range(0, 7)
                .Select(offset => last7Days.AddDays(offset))
                .Select(date => new DateMetricDto
                {
                    Date = date.ToString("MMM dd"),
                    Count = trendQuery.FirstOrDefault(t => t.Date == date)?.Count ?? 0
                })
                .ToList();

            analytics.WorkflowsCreatedTrend = trendData;

            // Top Contributors (Most Workflows)
            var topContributors = await _context.Workflows
                .Where(w => !w.IsDeleted && w.Owner != null)
                .GroupBy(w => new { w.OwnerId, w.Owner.FirstName, w.Owner.LastName })
                .Select(g => new UserMetricDto
                {
                    UserName = g.Key.FirstName + " " + g.Key.LastName,
                    Count = g.Count()
                })
                .OrderByDescending(u => u.Count)
                .Take(5)
                .ToListAsync();

            analytics.TopContributors = topContributors;

            return analytics;
        }
    }
}

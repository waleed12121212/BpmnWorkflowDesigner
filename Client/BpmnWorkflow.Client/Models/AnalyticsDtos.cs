using System;
using System.Collections.Generic;

namespace BpmnWorkflow.Client.Models
{
    public class DashboardAnalyticsDto
    {
        public int TotalWorkflows { get; set; }
        public int TotalUsers { get; set; }
        public int TotalComments { get; set; }
        public List<DateMetricDto> WorkflowsCreatedTrend { get; set; } = new();
        public List<UserMetricDto> TopContributors { get; set; } = new();
    }

    public class DateMetricDto
    {
        public string Date { get; set; }
        public int Count { get; set; }
    }

    public class UserMetricDto
    {
        public string UserName { get; set; }
        public int Count { get; set; }
    }
}

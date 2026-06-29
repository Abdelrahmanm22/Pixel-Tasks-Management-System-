namespace Tasks.Presentation.ViewModels
{
    public class EmployeeDashboardViewModel
    {
        public string? DisplayName { get; set; }

        // ─── KPI stat cards ──────────────────────────────────────────────────
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }

        public int CompletionRate => TotalTasks > 0 ? CompletedTasks * 100 / TotalTasks : 0;

        // ─── Charts ──────────────────────────────────────────────────────────
        // My tasks by Priority
        public int PriorityLow { get; set; }
        public int PriorityMedium { get; set; }
        public int PriorityHigh { get; set; }
        public int PriorityCritical { get; set; }

        // Tasks I completed over time
        public List<NamedCount> CompletionTrend { get; set; } = new();

        // ─── Tables / feeds ──────────────────────────────────────────────────
        public List<DashboardTaskItem> ActiveProgress { get; set; } = new();
        public List<DashboardTaskItem> UpcomingDeadlines { get; set; } = new();
        public List<ActivityItem> RecentMessages { get; set; } = new();
    }
}

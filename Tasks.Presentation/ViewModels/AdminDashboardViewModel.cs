namespace Tasks.Presentation.ViewModels
{
    public class AdminDashboardViewModel
    {
        // ─── KPI stat cards ──────────────────────────────────────────────────
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int ReviewedTasks { get; set; }
        public int OverdueTasks { get; set; }

        public int ActiveEmployees { get; set; }
        public int CorporationCount { get; set; }
        public int SectionCount { get; set; }

        // "Done" = work that is finished, whether or not it has been signed off.
        public int DoneTasks => CompletedTasks + ReviewedTasks;
        public int CompletionRate => TotalTasks > 0 ? DoneTasks * 100 / TotalTasks : 0;

        // ─── Charts ──────────────────────────────────────────────────────────
        // Tasks by Priority (Low / Medium / High / Critical)
        public int PriorityLow { get; set; }
        public int PriorityMedium { get; set; }
        public int PriorityHigh { get; set; }
        public int PriorityCritical { get; set; }

        // Tasks by Category (Normal / Point / Counter)
        public int CategoryNormal { get; set; }
        public int CategoryPoint { get; set; }
        public int CategoryCounter { get; set; }

        public List<NamedCount> CorporationWorkload { get; set; } = new();
        public List<NamedCount> TasksOverTime { get; set; } = new();

        // ─── Tables / feeds ──────────────────────────────────────────────────
        public List<DashboardTaskItem> OverdueAndDueSoon { get; set; } = new();
        public List<LeaderboardItem> TopEmployees { get; set; } = new();
        public List<ActivityItem> RecentActivity { get; set; } = new();
    }
}

using Tasks.Domain.Enums;

namespace Tasks.Presentation.ViewModels
{
    // Shared, concrete building blocks for the dashboards.
    // Concrete types (not anonymous) so they cross the runtime-compiled view boundary.

    /// <summary>A label/value pair used to feed charts (corporation workload, trends, etc.).</summary>
    public class NamedCount
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    /// <summary>A compact task row for "overdue / due soon" and "upcoming deadlines" tables.</summary>
    public class DashboardTaskItem
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? CorporationName { get; set; }
        public TaskCategory Category { get; set; }
        public PriorityLevel Priority { get; set; }
        public DateTime DueDate { get; set; }
        public WorkTaskStatus Status { get; set; }
        public int ProgressPercent { get; set; }
        public string? AssigneeName { get; set; }

        public bool IsOverdue => DueDate.Date < DateTime.UtcNow.Date && Status != WorkTaskStatus.Completed;
        public int DaysLeft => (int)(DueDate.Date - DateTime.UtcNow.Date).TotalDays;
    }

    /// <summary>A leaderboard row ranking employees by completed assignments.</summary>
    public class LeaderboardItem
    {
        public string? UserName { get; set; }
        public string? ImageUrl { get; set; }
        public Gender Gender { get; set; }
        public int CompletedCount { get; set; }
        public int TotalAssigned { get; set; }
        public int CompletionRate => TotalAssigned > 0 ? CompletedCount * 100 / TotalAssigned : 0;
    }

    /// <summary>A single entry in the recent-activity / recent-messages feed.</summary>
    public class ActivityItem
    {
        public string? UserName { get; set; }
        public string? ImageUrl { get; set; }
        public Gender Gender { get; set; }
        public int WorkTaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public CommentType Type { get; set; }
        public string? Preview { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

using Tasks.Domain.Enums;

namespace Tasks.Presentation.Helpers
{
    // Single source of truth for the status badge styling used across all task views.
    public static class TaskStatusBadge
    {
        public static (string Css, string Text) Resolve(WorkTaskStatus status) => status switch
        {
            WorkTaskStatus.Pending    => ("bg-soft-secondary text-secondary", "Pending"),
            WorkTaskStatus.InProgress => ("bg-soft-info text-info", "In Progress"),
            WorkTaskStatus.Completed  => ("bg-soft-success text-success", "Completed"),
            WorkTaskStatus.Reviewed   => ("bg-soft-primary text-primary", "Reviewed"),
            _ => ("bg-soft-secondary text-secondary", status.ToString())
        };
    }
}

using Tasks.Domain.Enums;

namespace Tasks.Presentation.ViewModels
{
    // Card row for the admin "My Created Tasks" view — surfaces assignee progress.
    public class CreatedTaskCardViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public TaskCategory Category { get; set; }
        public PriorityLevel Priority { get; set; }
        public DateTime DueDate { get; set; }
        public WorkTaskStatus Status { get; set; }
        public string? CorporationName { get; set; }

        public int AssigneeCount { get; set; }
        public int CompletedAssigneeCount { get; set; }

        // Average of assignee progress percents (0–100).
        public int OverallProgressPercent { get; set; }

        public List<AssigneeAvatarViewModel> Assignees { get; set; } = new();
    }

    public class AssigneeAvatarViewModel
    {
        public string? FullName { get; set; }
        public string? ImageUrl { get; set; }
        public Gender Gender { get; set; }
        public int ProgressPercent { get; set; }
        public WorkTaskStatus Status { get; set; }
    }
}

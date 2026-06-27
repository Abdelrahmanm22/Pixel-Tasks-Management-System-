using Tasks.Domain.Enums;

namespace Tasks.Presentation.ViewModels
{
    // The employee's working view of one assigned task.
    public class TaskWorkViewModel
    {
        public int WorkTaskId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public PriorityLevel Priority { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime DueDate { get; set; }
        public TaskCategory Category { get; set; }
        public string? TaskTypeName { get; set; }
        public string? CorporationName { get; set; }
        public string? SectionName { get; set; }
        public string? CreatedByName { get; set; }
        public WorkTaskStatus OverallStatus { get; set; }

        // This user's assignment.
        public int AssignmentId { get; set; }
        public WorkTaskStatus MyStatus { get; set; }

        // Counter-type.
        public int? TargetCount { get; set; }
        public int? CompletedCount { get; set; }

        // Point-type checklist (ordered).
        public List<TaskPointWorkViewModel> Points { get; set; } = new();

        public int ProgressPercent { get; set; }

        public List<TaskCommentViewModel> Comments { get; set; } = new();
        public CommentsPanelViewModel CommentsPanel { get; set; } = new();
    }

    public class TaskPointWorkViewModel
    {
        public int PointStatusId { get; set; }
        public int Order { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }
}

using Tasks.Domain.Enums;

namespace Tasks.Presentation.ViewModels
{
    // Slim row for the employee "My Tasks" list.
    public class MyTaskViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public TaskCategory Category { get; set; }
        public PriorityLevel Priority { get; set; }
        public DateTime DueDate { get; set; }
        public WorkTaskStatus MyStatus { get; set; }
        public string? CorporationName { get; set; }

        // 0–100, computed from points done / total or completed / target.
        public int ProgressPercent { get; set; }
    }
}

using Tasks.Domain.Enums;

namespace Tasks.Presentation.ViewModels
{
    // Per-assignee progress row shown on the admin task Details page.
    public class AssignmentProgressViewModel
    {
        public int AssignmentId { get; set; }
        public string? UserName { get; set; }
        public WorkTaskStatus Status { get; set; }
        public int ProgressPercent { get; set; }
        public int? CompletedCount { get; set; }
        public int? TargetCount { get; set; }
        public int PointsDone { get; set; }
        public int PointsTotal { get; set; }
    }
}

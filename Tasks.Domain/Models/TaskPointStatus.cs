namespace Tasks.Domain.Models
{
    /// <summary>
    /// Tracks the completion state of a single TaskPoint for a specific TaskAssignment.
    /// Each assigned user has their own independent completion record per point.
    /// </summary>
    public class TaskPointStatus : BaseModel
    {
        public bool IsCompleted   { get; set; } = false;
        public DateTime? CompletedAt { get; set; }

        // FKs
        public int TaskAssignmentId { get; set; }
        public int TaskPointId      { get; set; }

        // Navigation
        public TaskAssignment TaskAssignment { get; set; } = null!;
        public TaskPoint TaskPoint           { get; set; } = null!;
    }
}

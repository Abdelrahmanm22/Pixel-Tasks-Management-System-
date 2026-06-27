using Tasks.Domain.Enums;
using Tasks.Domain.Models.Identity;

namespace Tasks.Domain.Models
{
    public class TaskAssignment : BaseModel
    {
        public WorkTaskStatus Status { get; set; } = WorkTaskStatus.Pending;

        /// <summary>
        /// Tracks how many units this specific user has completed.
        /// Only relevant for Counter-type tasks; null otherwise.
        /// </summary>
        public int? CompletedCount { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // FKs
        public int WorkTaskId    { get; set; }
        public string UserId     { get; set; } = string.Empty;

        // Navigation
        public WorkTask WorkTask  { get; set; } = null!;
        public AppUser User       { get; set; } = null!;
        public ICollection<TaskPointStatus> PointStatuses { get; set; } = new HashSet<TaskPointStatus>();
    }
}

namespace Tasks.Domain.Models
{
    public class TaskPoint : BaseModel
    {
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Controls display order of points to the assigned person.
        /// Allows rearranging points without touching database IDs.
        /// </summary>
        public int Order { get; set; }

        // FK
        public int WorkTaskId { get; set; }

        // Navigation
        public WorkTask WorkTask { get; set; } = null!;
        public ICollection<TaskPointStatus> PointStatuses { get; set; } = new HashSet<TaskPointStatus>();
    }
}

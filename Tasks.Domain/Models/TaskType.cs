using Tasks.Domain.Enums;

namespace Tasks.Domain.Models
{
    public class TaskType : BaseModel
    {
        public string Name         { get; set; } = string.Empty;
        public TaskCategory Category { get; set; }

        // Navigation
        public ICollection<WorkTask> Tasks { get; set; } = new HashSet<WorkTask>();
    }
}

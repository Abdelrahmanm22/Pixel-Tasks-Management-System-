using Tasks.Domain.Enums;
using Tasks.Domain.Models.Identity;

namespace Tasks.Domain.Models
{
    public class WorkTask : BaseModel, ICodedEntity
    {
        public string Title       { get; set; } = string.Empty;
        public string Code        { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes      { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime DueDate    { get; set; }
        public PriorityLevel Priority { get; set; }
        public WorkTaskStatus Status  { get; set; } = WorkTaskStatus.Pending;

        /// <summary>
        /// Target count for Counter-type tasks. Null for Normal and Point types.
        /// </summary>
        public int? TargetCount { get; set; }

        // FKs
        public int TaskTypeId          { get; set; }
        public string CreatedByUserId  { get; set; } = string.Empty;
        public int CorporationId       { get; set; }
        /// <summary>
        /// Null means the task targets all sections (and all users) within the Corporation.
        /// </summary>
        public int? SectionId { get; set; }

        // Navigation
        public TaskType TaskType          { get; set; } = null!;
        public AppUser CreatedBy          { get; set; } = null!;
        public Corporation Corporation    { get; set; } = null!;
        public Section? Section           { get; set; }
        public ICollection<TaskAssignment> Assignments { get; set; } = new HashSet<TaskAssignment>();
        public ICollection<TaskPoint> Points           { get; set; } = new HashSet<TaskPoint>();
        public ICollection<TaskComment> Comments       { get; set; } = new HashSet<TaskComment>();
    }
}

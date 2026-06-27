using Tasks.Domain.Enums;
using Tasks.Domain.Models.Identity;

namespace Tasks.Domain.Models
{
    public class TaskComment : BaseModel
    {
        /// <summary>Text content — populated when Type is Text.</summary>
        public string? Content { get; set; }

        /// <summary>Stored file/image URL or path — populated when Type is Image or File.</summary>
        public string? FileUrl { get; set; }

        public CommentType Type    { get; set; }
        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;

        // FKs
        public int WorkTaskId        { get; set; }
        public string UserId         { get; set; } = string.Empty;
        public int TaskAssignmentId  { get; set; }

        // Navigation
        public WorkTask WorkTask            { get; set; } = null!;
        public AppUser User                 { get; set; } = null!;
        public TaskAssignment TaskAssignment { get; set; } = null!;
    }
}

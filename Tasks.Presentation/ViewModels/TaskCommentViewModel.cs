using Tasks.Domain.Enums;

namespace Tasks.Presentation.ViewModels
{
    public class TaskCommentViewModel
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public CommentType Type { get; set; }
        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserImageUrl { get; set; }

        // True when the current viewer authored this comment (right-aligned chat bubble).
        public bool IsMine { get; set; }
    }
}

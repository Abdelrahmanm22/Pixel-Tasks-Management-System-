using Tasks.Domain.Enums;
using Tasks.Domain.Models.Identity;

namespace Tasks.Domain.Models
{
    /// <summary>
    /// A single in-app notification addressed to one user. Title/Message/Url are
    /// rendered when the notification is created so display stays out of the views
    /// and future notification types can render however they like.
    /// </summary>
    public class Notification : BaseModel
    {
        /// <summary>Who receives the notification.</summary>
        public string RecipientUserId { get; set; } = string.Empty;

        /// <summary>Who triggered it (for avatar/name) — null for system events.</summary>
        public string? ActorUserId { get; set; }

        public NotificationType Type { get; set; }

        public string Title   { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        /// <summary>Relative deep-link to the relevant page (e.g. /Task/Work/12).</summary>
        public string? Url { get; set; }

        public bool IsRead        { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Optional reference to the originating task (for cleanup/linking).</summary>
        public int? WorkTaskId { get; set; }

        // Navigation
        public AppUser Recipient   { get; set; } = null!;
        public AppUser? Actor      { get; set; }
        public WorkTask? WorkTask  { get; set; }
    }
}

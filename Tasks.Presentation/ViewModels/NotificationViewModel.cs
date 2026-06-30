using Tasks.Domain.Enums;
using Tasks.Presentation.Helpers;

namespace Tasks.Presentation.ViewModels
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public NotificationType Type { get; set; }
        public string Title   { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Url    { get; set; }
        public bool IsRead    { get; set; }
        public DateTime CreatedAt { get; set; }

        // Actor (who triggered it) — for the avatar and name.
        public string? ActorName     { get; set; }
        public string? ActorImageUrl { get; set; }
        public Gender ActorGender     { get; set; }

        public string AvatarSrc => AvatarHelper.Resolve(ActorImageUrl, ActorGender);

        // Boxicon + colour per type, used by both the dropdown and history page.
        public string Icon => Type switch
        {
            NotificationType.TaskAssigned    => "bx-task",
            NotificationType.NewComment      => "bx-message-square-dots",
            NotificationType.TaskNeedsReview => "bx-check-double",
            _ => "bx-bell"
        };

        public string ColorClass => Type switch
        {
            NotificationType.TaskAssigned    => "bg-primary",
            NotificationType.NewComment      => "bg-info",
            NotificationType.TaskNeedsReview => "bg-success",
            _ => "bg-secondary"
        };

        public string TimeAgo
        {
            get
            {
                var span = DateTime.UtcNow - CreatedAt;
                if (span.TotalSeconds < 60)  return "just now";
                if (span.TotalMinutes < 60)  return $"{(int)span.TotalMinutes} min ago";
                if (span.TotalHours < 24)    return $"{(int)span.TotalHours} hr ago";
                if (span.TotalDays < 7)      return $"{(int)span.TotalDays} day{((int)span.TotalDays == 1 ? "" : "s")} ago";
                return CreatedAt.ToLocalTime().ToString("dd/MM/yyyy");
            }
        }
    }
}

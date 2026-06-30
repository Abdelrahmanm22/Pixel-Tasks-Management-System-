using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.NotificationSpec
{
    public class UnreadNotificationByUserSpec : BaseSpecifications<Notification>
    {
        // A user's unread notifications — used for the badge count and "mark all as read".
        public UnreadNotificationByUserSpec(string userId)
            : base(n => n.RecipientUserId == userId && !n.IsRead)
        {
        }
    }
}

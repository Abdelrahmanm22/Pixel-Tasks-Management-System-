using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.NotificationSpec
{
    public class NotificationByUserSpec : BaseSpecifications<Notification>
    {
        // A user's notifications, newest first, with the actor for avatar/name display.
        public NotificationByUserSpec(string userId) : base(n => n.RecipientUserId == userId)
        {
            AddInclude("Actor");
            SetOrderByDesc(n => n.CreatedAt);
        }

        // Paginated overload — used by the history page and the bell's "recent" fetch.
        public NotificationByUserSpec(string userId, int skip, int take) : this(userId)
        {
            ApplyPagination(skip, take);
        }
    }
}

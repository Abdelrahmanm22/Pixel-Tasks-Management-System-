using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.NotificationSpec
{
    public class NotificationByIdSpec : BaseSpecifications<Notification>
    {
        public NotificationByIdSpec(int id) : base(n => n.Id == id)
        {
        }
    }
}

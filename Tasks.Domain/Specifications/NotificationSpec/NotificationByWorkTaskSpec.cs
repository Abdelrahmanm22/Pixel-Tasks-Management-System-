using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.NotificationSpec
{
    public class NotificationByWorkTaskSpec : BaseSpecifications<Notification>
    {
        // All notifications referencing a task — used to clear them before the task is deleted
        // (the WorkTask FK is NoAction to avoid cascade-path cycles).
        public NotificationByWorkTaskSpec(int workTaskId) : base(n => n.WorkTaskId == workTaskId)
        {
        }
    }
}

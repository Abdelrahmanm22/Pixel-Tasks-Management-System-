using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.WorkTaskSpec
{
    // Tasks created by the given user (admin "My Created Tasks" cards).
    public class WorkTaskByCreatorSpec : BaseSpecifications<WorkTask>
    {
        public WorkTaskByCreatorSpec(string userId)
            : base(t => t.CreatedByUserId == userId)
        {
            AddInclude("TaskType");
            AddInclude("Corporation");
            AddInclude("Section");
            AddInclude("Points");
            AddInclude("Assignments.User");
            AddInclude("Assignments.PointStatuses");
            SetOrderByDesc(t => t.RequestDate);
        }
    }
}

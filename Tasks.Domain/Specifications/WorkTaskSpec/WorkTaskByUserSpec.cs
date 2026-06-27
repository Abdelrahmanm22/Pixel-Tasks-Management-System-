using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.WorkTaskSpec
{
    // Tasks that the given user is assigned to (employee "My Tasks" list).
    public class WorkTaskByUserSpec : BaseSpecifications<WorkTask>
    {
        public WorkTaskByUserSpec(string userId)
            : base(t => t.Assignments.Any(a => a.UserId == userId))
        {
            AddInclude("TaskType");
            AddInclude("Corporation");
            AddInclude("Section");
            AddInclude("Points");
            AddInclude("Assignments.PointStatuses");
            SetOrderByDesc(t => t.RequestDate);
        }
    }
}

using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.WorkTaskSpec
{
    public class WorkTaskSpec : BaseSpecifications<WorkTask>
    {
        // All tasks (admin Index) — lightweight graph for the listing.
        public WorkTaskSpec() : base()
        {
            AddInclude("TaskType");
            AddInclude("Corporation");
            AddInclude("Section");
            AddInclude("CreatedBy");
            AddInclude("Assignments");
            SetOrderByDesc(t => t.RequestDate);
        }

        // Single task with the full graph (Details / Edit).
        public WorkTaskSpec(int id) : base(t => t.Id == id)
        {
            AddInclude("TaskType");
            AddInclude("Corporation");
            AddInclude("Section");
            AddInclude("CreatedBy");
            AddInclude("Points");
            AddInclude("Assignments.User");
            AddInclude("Assignments.PointStatuses");
            AddInclude("Comments.User");
        }
    }
}

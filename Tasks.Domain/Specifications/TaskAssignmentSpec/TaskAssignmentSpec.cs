using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.TaskAssignmentSpec
{
    // The single assignment for a given task + user (employee work view & progress endpoints).
    public class TaskAssignmentSpec : BaseSpecifications<TaskAssignment>
    {
        public TaskAssignmentSpec(int workTaskId, string userId)
            : base(a => a.WorkTaskId == workTaskId && a.UserId == userId)
        {
            AddInclude("WorkTask.TaskType");
            AddInclude("WorkTask.Points");
            AddInclude("PointStatuses.TaskPoint");
        }
    }
}

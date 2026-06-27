using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.TaskCommentSpec
{
    public class TaskCommentByAssignmentSpec : BaseSpecifications<TaskComment>
    {
        public TaskCommentByAssignmentSpec(int taskAssignmentId)
            : base(c => c.TaskAssignmentId == taskAssignmentId)
        {
            AddInclude("User");
            SetOrderBy(c => c.CreatedAt);
        }
    }
}

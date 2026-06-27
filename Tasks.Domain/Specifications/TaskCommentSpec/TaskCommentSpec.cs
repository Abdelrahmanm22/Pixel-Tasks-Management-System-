using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.TaskCommentSpec
{
    public class TaskCommentSpec : BaseSpecifications<TaskComment>
    {
        // All comments for a task, oldest first (chat order), with the author.
        public TaskCommentSpec(int workTaskId) : base(c => c.WorkTaskId == workTaskId)
        {
            AddInclude("User");
            SetOrderBy(c => c.CreatedAt);
        }
    }
}

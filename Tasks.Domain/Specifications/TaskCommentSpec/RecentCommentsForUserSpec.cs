using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.TaskCommentSpec
{
    // Newest comments posted by someone else inside the given user's own task
    // threads — drives the employee "Recent messages" widget.
    public class RecentCommentsForUserSpec : BaseSpecifications<TaskComment>
    {
        public RecentCommentsForUserSpec(string userId, int take)
            : base(c => c.TaskAssignment.UserId == userId && c.UserId != userId)
        {
            AddInclude("User");
            AddInclude("WorkTask");
            SetOrderByDesc(c => c.CreatedAt);
            ApplyPagination(0, take);
        }
    }
}

using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.TaskCommentSpec
{
    // Newest comments across all tasks — drives the admin "Recent activity" feed.
    public class RecentCommentsSpec : BaseSpecifications<TaskComment>
    {
        public RecentCommentsSpec(int take) : base()
        {
            AddInclude("User");
            AddInclude("WorkTask");
            SetOrderByDesc(c => c.CreatedAt);
            ApplyPagination(0, take);
        }
    }
}

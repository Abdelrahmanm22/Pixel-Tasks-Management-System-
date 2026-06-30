using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.WorkTaskSpec
{
    // Lightweight graph for dashboard aggregates: status / priority / category /
    // corporation workload and per-assignee leaderboard. Includes assignment users
    // so we can attribute completed work without extra round-trips.
    public class DashboardWorkTaskSpec : BaseSpecifications<WorkTask>
    {
        public DashboardWorkTaskSpec() : base()
        {
            AddInclude("TaskType");
            AddInclude("Corporation");
            AddInclude("Assignments.User");
            SetOrderByDesc(t => t.RequestDate);
        }

        // Scoped to a single admin's created tasks (admin dashboard isolation).
        public DashboardWorkTaskSpec(string creatorUserId) : base(t => t.CreatedByUserId == creatorUserId)
        {
            AddInclude("TaskType");
            AddInclude("Corporation");
            AddInclude("Assignments.User");
            SetOrderByDesc(t => t.RequestDate);
        }
    }
}

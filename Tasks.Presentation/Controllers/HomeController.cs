using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tasks.Domain;
using Tasks.Domain.Authorization;
using Tasks.Domain.Enums;
using Tasks.Domain.Models;
using Tasks.Domain.Models.Identity;
using Tasks.Domain.Specifications.CorporationSpec;
using Tasks.Domain.Specifications.SectionSpec;
using Tasks.Domain.Specifications.TaskCommentSpec;
using Tasks.Domain.Specifications.WorkTaskSpec;
using Tasks.Presentation.ViewModels;
using Test.ViewModels;

namespace Tasks.Presentation.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private const int FeedSize = 8;
        private const int TableSize = 6;
        private const int TrendMonths = 6;

        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            ILogger<HomeController> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole(Roles.Admin))
                return View("AdminDashboard", await BuildAdminDashboardAsync());

            if (User.IsInRole(Roles.Employee))
                return View("EmployeeDashboard", await BuildEmployeeDashboardAsync());

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // ─── Admin dashboard ─────────────────────────────────────────────────

        private async Task<AdminDashboardViewModel> BuildAdminDashboardAsync()
        {
            var tasks = (await _unitOfWork.Repository<WorkTask>().GetAllAsync(new DashboardWorkTaskSpec())).ToList();
            var corporations = await _unitOfWork.Repository<Corporation>().GetAllAsync(new CorporationSpec());
            var sections = await _unitOfWork.Repository<Section>().GetAllAsync(new SectionSpec());
            var employees = await _userManager.GetUsersInRoleAsync(Roles.Employee);
            var recentComments = await _unitOfWork.Repository<TaskComment>().GetAllAsync(new RecentCommentsSpec(FeedSize));

            var today = DateTime.UtcNow.Date;

            var vm = new AdminDashboardViewModel
            {
                TotalTasks = tasks.Count,
                PendingTasks = tasks.Count(t => t.Status == WorkTaskStatus.Pending),
                InProgressTasks = tasks.Count(t => t.Status == WorkTaskStatus.InProgress),
                CompletedTasks = tasks.Count(t => t.Status == WorkTaskStatus.Completed),
                OverdueTasks = tasks.Count(t => t.DueDate.Date < today && t.Status != WorkTaskStatus.Completed),

                ActiveEmployees = employees.Count(u => u.IsActive),
                CorporationCount = corporations.Count(),
                SectionCount = sections.Count(),

                PriorityLow = tasks.Count(t => t.Priority == PriorityLevel.Low),
                PriorityMedium = tasks.Count(t => t.Priority == PriorityLevel.Medium),
                PriorityHigh = tasks.Count(t => t.Priority == PriorityLevel.High),
                PriorityCritical = tasks.Count(t => t.Priority == PriorityLevel.Critical),

                CategoryNormal = tasks.Count(t => t.TaskType?.Category == TaskCategory.Normal),
                CategoryPoint = tasks.Count(t => t.TaskType?.Category == TaskCategory.Point),
                CategoryCounter = tasks.Count(t => t.TaskType?.Category == TaskCategory.Counter),

                CorporationWorkload = tasks
                    .Where(t => t.Corporation != null)
                    .GroupBy(t => t.Corporation!.Name)
                    .Select(g => new NamedCount { Name = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(8)
                    .ToList(),

                TasksOverTime = MonthlyBuckets(tasks.Select(t => t.RequestDate), TrendMonths),
            };

            // Overdue + due within the next 7 days, soonest first
            vm.OverdueAndDueSoon = tasks
                .Where(t => t.Status != WorkTaskStatus.Completed && t.DueDate.Date <= today.AddDays(7))
                .OrderBy(t => t.DueDate)
                .Take(FeedSize)
                .Select(t => new DashboardTaskItem
                {
                    Id = t.Id,
                    Code = t.Code,
                    Title = t.Title,
                    CorporationName = t.Corporation?.Name,
                    Category = t.TaskType?.Category ?? TaskCategory.Normal,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    Status = t.Status,
                    ProgressPercent = AssigneeCompletionPercent(t),
                    AssigneeName = DescribeAssignees(t)
                })
                .ToList();

            // Leaderboard: rank employees by completed assignments
            vm.TopEmployees = tasks
                .SelectMany(t => t.Assignments)
                .Where(a => a.User != null)
                .GroupBy(a => a.User!)
                .Select(g => new LeaderboardItem
                {
                    UserName = g.Key.FullName,
                    ImageUrl = g.Key.ImageUrl,
                    Gender = g.Key.Gender,
                    CompletedCount = g.Count(a => a.Status == WorkTaskStatus.Completed),
                    TotalAssigned = g.Count()
                })
                .OrderByDescending(x => x.CompletedCount)
                .ThenByDescending(x => x.CompletionRate)
                .Take(5)
                .ToList();

            vm.RecentActivity = recentComments.Select(ToActivityItem).ToList();

            return vm;
        }

        // ─── Employee dashboard ──────────────────────────────────────────────

        private async Task<EmployeeDashboardViewModel> BuildEmployeeDashboardAsync()
        {
            var userId = _userManager.GetUserId(User)!;
            var user = await _userManager.GetUserAsync(User);
            var tasks = (await _unitOfWork.Repository<WorkTask>().GetAllAsync(new WorkTaskByUserSpec(userId))).ToList();
            var recentComments = await _unitOfWork.Repository<TaskComment>().GetAllAsync(new RecentCommentsForUserSpec(userId, TableSize));

            var today = DateTime.UtcNow.Date;

            // Pair each task with the current user's own assignment.
            var mine = tasks
                .Select(t => new { Task = t, Assignment = t.Assignments.FirstOrDefault(a => a.UserId == userId) })
                .Where(x => x.Assignment != null)
                .ToList();

            var vm = new EmployeeDashboardViewModel
            {
                DisplayName = user?.FirstName,

                TotalTasks = mine.Count,
                PendingTasks = mine.Count(x => x.Assignment!.Status == WorkTaskStatus.Pending),
                InProgressTasks = mine.Count(x => x.Assignment!.Status == WorkTaskStatus.InProgress),
                CompletedTasks = mine.Count(x => x.Assignment!.Status == WorkTaskStatus.Completed),
                OverdueTasks = mine.Count(x => x.Task.DueDate.Date < today && x.Assignment!.Status != WorkTaskStatus.Completed),

                PriorityLow = mine.Count(x => x.Task.Priority == PriorityLevel.Low),
                PriorityMedium = mine.Count(x => x.Task.Priority == PriorityLevel.Medium),
                PriorityHigh = mine.Count(x => x.Task.Priority == PriorityLevel.High),
                PriorityCritical = mine.Count(x => x.Task.Priority == PriorityLevel.Critical),

                CompletionTrend = MonthlyBuckets(
                    mine.Where(x => x.Assignment!.Status == WorkTaskStatus.Completed).Select(x => x.Task.DueDate),
                    TrendMonths),
            };

            // Active tasks (not yet completed) with live progress, soonest due first
            vm.ActiveProgress = mine
                .Where(x => x.Assignment!.Status != WorkTaskStatus.Completed)
                .OrderBy(x => x.Task.DueDate)
                .Take(TableSize)
                .Select(x => ToTaskItem(x.Task, x.Assignment!))
                .ToList();

            // Upcoming deadlines — same pool, but always show the nearest dates
            vm.UpcomingDeadlines = mine
                .Where(x => x.Assignment!.Status != WorkTaskStatus.Completed)
                .OrderBy(x => x.Task.DueDate)
                .Take(TableSize)
                .Select(x => ToTaskItem(x.Task, x.Assignment!))
                .ToList();

            vm.RecentMessages = recentComments.Select(ToActivityItem).ToList();

            return vm;
        }

        // ─── Mapping / calculation helpers ───────────────────────────────────

        private DashboardTaskItem ToTaskItem(WorkTask task, TaskAssignment assignment) => new()
        {
            Id = task.Id,
            Code = task.Code,
            Title = task.Title,
            CorporationName = task.Corporation?.Name,
            Category = task.TaskType?.Category ?? TaskCategory.Normal,
            Priority = task.Priority,
            DueDate = task.DueDate,
            Status = assignment.Status,
            ProgressPercent = ProgressPercent(task, assignment)
        };

        private static ActivityItem ToActivityItem(TaskComment c) => new()
        {
            UserName = c.User?.FullName,
            ImageUrl = c.User?.ImageUrl,
            Gender = c.User?.Gender ?? Gender.Male,
            WorkTaskId = c.WorkTaskId,
            TaskTitle = c.WorkTask?.Title ?? "Task",
            Type = c.Type,
            Preview = c.Type switch
            {
                CommentType.Image => "Sent an image",
                CommentType.File => "Sent a file",
                _ => Truncate(c.Content, 80)
            },
            CreatedAt = c.CreatedAt
        };

        // Per-assignee progress for a single user's assignment (points / counter / normal).
        private static int ProgressPercent(WorkTask task, TaskAssignment assignment)
        {
            var category = task.TaskType?.Category ?? TaskCategory.Normal;
            switch (category)
            {
                case TaskCategory.Point:
                    var total = task.Points.Count;
                    if (total == 0) return 0;
                    return assignment.PointStatuses.Count(ps => ps.IsCompleted) * 100 / total;
                case TaskCategory.Counter:
                    var target = task.TargetCount ?? 0;
                    if (target == 0) return 0;
                    return Math.Min(100, (assignment.CompletedCount ?? 0) * 100 / target);
                default:
                    return assignment.Status == WorkTaskStatus.Completed ? 100 : 0;
            }
        }

        // Aggregate task progress as the share of assignees that finished — cheap,
        // needs only assignment statuses (no point-status round-trips).
        private static int AssigneeCompletionPercent(WorkTask task)
        {
            if (task.Assignments.Count == 0) return 0;
            var done = task.Assignments.Count(a => a.Status == WorkTaskStatus.Completed);
            return done * 100 / task.Assignments.Count;
        }

        private static string DescribeAssignees(WorkTask task)
        {
            var count = task.Assignments.Count;
            if (count == 0) return "Unassigned";
            if (count == 1) return task.Assignments.First().User?.FullName ?? "1 assignee";
            return $"{count} assignees";
        }

        private static List<NamedCount> MonthlyBuckets(IEnumerable<DateTime> dates, int months)
        {
            var list = dates.ToList();
            var now = DateTime.UtcNow;
            var buckets = new List<NamedCount>();
            for (var i = months - 1; i >= 0; i--)
            {
                var d = now.AddMonths(-i);
                buckets.Add(new NamedCount
                {
                    Name = d.ToString("MMM"),
                    Count = list.Count(x => x.Year == d.Year && x.Month == d.Month)
                });
            }
            return buckets;
        }

        private static string? Truncate(string? value, int max)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= max ? value : value[..max].TrimEnd() + "…";
        }
    }
}

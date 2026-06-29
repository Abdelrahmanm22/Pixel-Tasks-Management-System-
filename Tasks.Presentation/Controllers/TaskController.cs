using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tasks.Domain;
using Tasks.Domain.Authorization;
using Tasks.Domain.Enums;
using Tasks.Domain.Models;
using Tasks.Domain.Models.Identity;
using Tasks.Domain.Services;
using Tasks.Domain.Specifications.CorporationSpec;
using Tasks.Domain.Specifications.SectionSpec;
using Tasks.Domain.Specifications.TaskTypeSpec;
using Tasks.Domain.Specifications.TaskCommentSpec;
using Tasks.Domain.Specifications.WorkTaskSpec;
using Tasks.Presentation.Helpers;
using Tasks.Presentation.ViewModels;

namespace Tasks.Presentation.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private const string CommentsFolder = "TaskComments";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICodeGeneratorService _codeGenerator;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<TaskController> _logger;

        public TaskController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICodeGeneratorService codeGenerator,
            UserManager<AppUser> userManager,
            ILogger<TaskController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _codeGenerator = codeGenerator;
            _userManager = userManager;
            _logger = logger;
        }

        // ─── Admin: list / details ───────────────────────────────────────────

        [Authorize(Policy = Permissions.Tasks.ViewAll)]
        public async Task<IActionResult> Index()
        {
            var tasks = await _unitOfWork.Repository<WorkTask>().GetAllAsync(new WorkTaskSpec());
            var viewModels = _mapper.Map<IEnumerable<WorkTask>, IEnumerable<WorkTaskViewModel>>(tasks);
            return View(viewModels);
        }

        [Authorize(Policy = Permissions.Tasks.ViewAll)]
        public async Task<IActionResult> Details(int id, int? assignmentId)
        {
            var task = await _unitOfWork.Repository<WorkTask>().GetByIdAsync(new WorkTaskSpec(id));
            if (task is null)
                return NotFound();

            var vm = _mapper.Map<WorkTask, WorkTaskViewModel>(task);
            vm.Points = task.Points.OrderBy(p => p.Order)
                .Select(p => new TaskPointViewModel { Id = p.Id, Description = p.Description, Order = p.Order })
                .ToList();

            var assignments = BuildAssignmentProgress(task);
            ViewBag.Assignments = assignments;

            // Select the thread: use the provided assignmentId or fall back to the first assignee
            var selectedAssignment = assignmentId.HasValue
                ? task.Assignments.FirstOrDefault(a => a.Id == assignmentId.Value)
                : task.Assignments.FirstOrDefault();

            List<TaskCommentViewModel> comments = new();
            int selectedAssignmentId = selectedAssignment?.Id ?? 0;
            if (selectedAssignment is not null)
            {
                var threadComments = await _unitOfWork.Repository<TaskComment>()
                    .GetAllAsync(new TaskCommentByAssignmentSpec(selectedAssignment.Id));
                comments = BuildComments(threadComments);
            }

            ViewBag.CommentsPanel = new CommentsPanelViewModel
            {
                WorkTaskId = task.Id,
                TaskAssignmentId = selectedAssignmentId,
                Comments = comments,
                IsAdminView = true,
                Assignees = assignments.Select(a => new AssigneeDropdownItem
                {
                    AssignmentId = a.AssignmentId,
                    UserName = a.UserName ?? "Unknown"
                }).ToList(),
                ReturnUrl = Url.Action("Details", new { id, assignmentId = selectedAssignmentId })!
            };

            return View(vm);
        }

        // ─── Admin: create ───────────────────────────────────────────────────

        [Authorize(Policy = Permissions.Tasks.Create)]
        public async Task<IActionResult> Create()
        {
            var model = new WorkTaskViewModel();
            await PopulateLookupsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Tasks.Create)]
        public async Task<IActionResult> Create(WorkTaskViewModel model)
        {
            var currentUserId = _userManager.GetUserId(User);

            _logger.LogInformation(
                "User {UserId} started creating a task. Title: {Title}",
                currentUserId,
                model.Title
            );
            var category = await ResolveCategoryAsync(model.TaskTypeId);
            ValidateByCategory(model, category);

            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync(model);
                return View(model);
            }

            var employeeIds = await ResolveAssigneeIdsAsync(model.CorporationId, model.SectionId, model.SelectedUserIds);
            if (employeeIds.Count == 0)
            {
                ModelState.AddModelError(nameof(model.SelectedUserIds), "Please assign the task to at least one employee.");
                await PopulateLookupsAsync(model);
                return View(model);
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var task = _mapper.Map<WorkTaskViewModel, WorkTask>(model);
                task.Code = await _codeGenerator.GenerateCodeAsync<WorkTask>("PXW");
                task.Status = WorkTaskStatus.Pending;
                task.CreatedByUserId = _userManager.GetUserId(User)!;
                task.TargetCount = category == TaskCategory.Counter ? model.TargetCount : null;

                await _unitOfWork.Repository<WorkTask>().AddAsync(task);
                await _unitOfWork.CompleteAsync();

                // Points (Point-type only)
                var points = new List<TaskPoint>();
                if (category == TaskCategory.Point)
                {
                    var order = 1;
                    foreach (var p in model.Points.Where(p => !string.IsNullOrWhiteSpace(p.Description)))
                    {
                        var point = new TaskPoint { Description = p.Description.Trim(), Order = order++, WorkTaskId = task.Id };
                        await _unitOfWork.Repository<TaskPoint>().AddAsync(point);
                        points.Add(point);
                    }
                    await _unitOfWork.CompleteAsync();
                }

                // One assignment per employee
                foreach (var userId in employeeIds)
                {
                    var assignment = new TaskAssignment
                    {
                        WorkTaskId = task.Id,
                        UserId = userId,
                        Status = WorkTaskStatus.Pending,
                        AssignedAt = DateTime.UtcNow,
                        CompletedCount = category == TaskCategory.Counter ? 0 : null
                    };
                    await _unitOfWork.Repository<TaskAssignment>().AddAsync(assignment);
                    await _unitOfWork.CompleteAsync();

                    // Seed a per-point status for Point-type tasks
                    foreach (var point in points)
                    {
                        await _unitOfWork.Repository<TaskPointStatus>().AddAsync(new TaskPointStatus
                        {
                            TaskAssignmentId = assignment.Id,
                            TaskPointId = point.Id,
                            IsCompleted = false
                        });
                    }
                }
                await _unitOfWork.CompleteAsync();

                await _unitOfWork.CommitTransactionAsync();
                TempData["Success"] = $"Task \"{task.Title}\" created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(
                    ex,
                    "Error creating task. UserId: {UserId}, Title: {Title}",
                    currentUserId,
                    model.Title
                );
                TempData["Error"] = "An error occurred while creating the task.";
                await PopulateLookupsAsync(model);
                return View(model);
            }
        }

        // ─── Admin: edit ─────────────────────────────────────────────────────

        [Authorize(Policy = Permissions.Tasks.Create)]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _unitOfWork.Repository<WorkTask>().GetByIdAsync(new WorkTaskSpec(id));
            if (task is null)
                return NotFound();

            var vm = _mapper.Map<WorkTask, WorkTaskViewModel>(task);
            vm.SelectedUserIds = task.Assignments.Select(a => a.UserId).ToList();
            vm.Points = task.Points.OrderBy(p => p.Order)
                .Select(p => new TaskPointViewModel { Id = p.Id, Description = p.Description, Order = p.Order })
                .ToList();

            await PopulateLookupsAsync(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Tasks.Create)]
        public async Task<IActionResult> Edit(int id, WorkTaskViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            var task = await _unitOfWork.Repository<WorkTask>().GetByIdAsync(new WorkTaskSpec(id));
            if (task is null)
                return NotFound();

            var category = task.TaskType.Category; // task type is immutable after creation
            ValidateByCategory(model, category);

            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync(model);
                return View(model);
            }

            var employeeIds = await ResolveAssigneeIdsAsync(model.CorporationId, model.SectionId, model.SelectedUserIds);
            if (employeeIds.Count == 0)
            {
                ModelState.AddModelError(nameof(model.SelectedUserIds), "Please assign the task to at least one employee.");
                await PopulateLookupsAsync(model);
                return View(model);
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Descriptive fields — always editable
                task.Title = model.Title;
                task.Description = model.Description;
                task.Notes = model.Notes;
                task.RequestDate = model.RequestDate;
                task.DueDate = model.DueDate;
                task.Priority = model.Priority;
                if (category == TaskCategory.Counter)
                    task.TargetCount = model.TargetCount;

                _unitOfWork.Repository<WorkTask>().Update(task);
                await _unitOfWork.CompleteAsync();

                await ReconcilePointsAsync(task, model, category);
                await ReconcileAssignmentsAsync(task, employeeIds, category);

                // Refresh aggregate state after structural changes
                var refreshed = await _unitOfWork.Repository<WorkTask>().GetByIdAsync(new WorkTaskSpec(id));
                RecomputeAllStatuses(refreshed);
                _unitOfWork.Repository<WorkTask>().Update(refreshed);
                await _unitOfWork.CompleteAsync();

                await _unitOfWork.CommitTransactionAsync();
                TempData["Success"] = $"Task \"{task.Title}\" updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                TempData["Error"] = "An error occurred while updating the task.";
                await PopulateLookupsAsync(model);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Tasks.Create)]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _unitOfWork.Repository<WorkTask>().GetByIdAsync(new WorkTaskSpec(id));
            if (task is null)
                return Json(new { success = false, message = "Task not found." });

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // TaskPoint → TaskPointStatus is NoAction, so clear point statuses first
                foreach (var assignment in task.Assignments)
                    foreach (var ps in assignment.PointStatuses.ToList())
                        _unitOfWork.Repository<TaskPointStatus>().Delete(ps);
                await _unitOfWork.CompleteAsync();

                // TaskAssignment → TaskComment is NoAction, so delete comments before assignments
                foreach (var comment in task.Comments.ToList())
                    _unitOfWork.Repository<TaskComment>().Delete(comment);
                await _unitOfWork.CompleteAsync();

                _unitOfWork.Repository<WorkTask>().Delete(task);
                await _unitOfWork.CompleteAsync();

                await _unitOfWork.CommitTransactionAsync();
                return Json(new { success = true, message = $"Task \"{task.Title}\" deleted successfully." });
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Json(new { success = false, message = "An error occurred while deleting the task." });
            }
        }

        // ─── Employee: my tasks / work view ──────────────────────────────────

        [Authorize(Policy = Permissions.Tasks.ViewAssigned)]
        public async Task<IActionResult> MyTasks()
        {
            var userId = _userManager.GetUserId(User)!;
            var tasks = await _unitOfWork.Repository<WorkTask>().GetAllAsync(new WorkTaskByUserSpec(userId));

            var list = BuildMyTasks(tasks, userId)
                .Where(t => t.MyStatus != WorkTaskStatus.Reviewed)
                .ToList();

            return View(list);
        }

        [Authorize(Policy = Permissions.Tasks.ViewAssigned)]
        public async Task<IActionResult> AllMyTasks()
        {
            var userId = _userManager.GetUserId(User)!;
            var tasks = await _unitOfWork.Repository<WorkTask>().GetAllAsync(new WorkTaskByUserSpec(userId));

            var list = BuildMyTasks(tasks, userId).ToList();
            return View(list);
        }

        [Authorize(Policy = Permissions.Tasks.ViewAll)]
        public async Task<IActionResult> MyCreatedTasks()
        {
            var userId = _userManager.GetUserId(User)!;
            var tasks = await _unitOfWork.Repository<WorkTask>().GetAllAsync(new WorkTaskByCreatorSpec(userId));

            var list = tasks
                .Where(t => t.Status != WorkTaskStatus.Reviewed)
                .Select(t =>
                {
                    var category = t.TaskType.Category;
                    var assignees = t.Assignments.Select(a => new AssigneeAvatarViewModel
                    {
                        FullName = a.User?.FullName,
                        ImageUrl = a.User?.ImageUrl,
                        Gender = a.User?.Gender ?? Gender.Male,
                        ProgressPercent = ProgressPercent(category, a, t),
                        Status = a.Status
                    }).ToList();

                    return new CreatedTaskCardViewModel
                    {
                        Id = t.Id,
                        Code = t.Code,
                        Title = t.Title,
                        Category = category,
                        Priority = t.Priority,
                        DueDate = t.DueDate,
                        Status = t.Status,
                        CorporationName = t.Corporation?.Name,
                        AssigneeCount = assignees.Count,
                        CompletedAssigneeCount = assignees.Count(a => a.Status == WorkTaskStatus.Completed),
                        OverallProgressPercent = assignees.Count > 0
                            ? (int)Math.Round(assignees.Average(a => a.ProgressPercent))
                            : 0,
                        Assignees = assignees
                    };
                }).ToList();

            return View(list);
        }

        private IEnumerable<MyTaskViewModel> BuildMyTasks(IEnumerable<WorkTask> tasks, string userId) =>
            tasks.Select(t =>
            {
                var assignment = t.Assignments.First(a => a.UserId == userId);
                return new MyTaskViewModel
                {
                    Id = t.Id,
                    Code = t.Code,
                    Title = t.Title,
                    Category = t.TaskType.Category,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    MyStatus = assignment.Status,
                    CorporationName = t.Corporation?.Name,
                    ProgressPercent = ProgressPercent(t.TaskType.Category, assignment, t)
                };
            });

        [Authorize(Policy = Permissions.Tasks.ViewAssigned)]
        public async Task<IActionResult> Work(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var task = await _unitOfWork.Repository<WorkTask>().GetByIdAsync(new WorkTaskSpec(id));
            if (task is null)
                return NotFound();

            var assignment = task.Assignments.FirstOrDefault(a => a.UserId == userId);
            if (assignment is null)
                return Forbid();

            var category = task.TaskType.Category;
            var vm = new TaskWorkViewModel
            {
                WorkTaskId = task.Id,
                Code = task.Code,
                Title = task.Title,
                Description = task.Description,
                Notes = task.Notes,
                Priority = task.Priority,
                RequestDate = task.RequestDate,
                DueDate = task.DueDate,
                Category = category,
                TaskTypeName = task.TaskType.Name,
                CorporationName = task.Corporation?.Name,
                SectionName = task.Section?.Name,
                CreatedByName = task.CreatedBy?.FullName,
                OverallStatus = task.Status,
                AssignmentId = assignment.Id,
                MyStatus = assignment.Status,
                TargetCount = task.TargetCount,
                CompletedCount = assignment.CompletedCount,
                ProgressPercent = ProgressPercent(category, assignment, task),
                Points = assignment.PointStatuses
                    .Select(ps => new TaskPointWorkViewModel
                    {
                        PointStatusId = ps.Id,
                        Order = ps.TaskPoint?.Order ?? task.Points.FirstOrDefault(p => p.Id == ps.TaskPointId)?.Order ?? 0,
                        Description = ps.TaskPoint?.Description ?? task.Points.FirstOrDefault(p => p.Id == ps.TaskPointId)?.Description ?? string.Empty,
                        IsCompleted = ps.IsCompleted
                    })
                    .OrderBy(p => p.Order)
                    .ToList(),
                Comments = await BuildCommentsForAssignmentAsync(assignment.Id)
            };

            vm.CommentsPanel = new CommentsPanelViewModel
            {
                WorkTaskId = task.Id,
                TaskAssignmentId = assignment.Id,
                Comments = vm.Comments,
                IsAdminView = false,
                ReturnUrl = Url.Action("Work", new { id })!
            };

            return View(vm);
        }

        // ─── Employee: progress endpoints ────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Tasks.UpdateProgress)]
        public async Task<IActionResult> TogglePoint(int workTaskId, int pointStatusId)
        {
            var (task, assignment) = await LoadOwnedAsync(workTaskId);
            if (assignment is null)
                return Json(new { success = false, message = "Not authorized for this task." });

            if (assignment.Status == WorkTaskStatus.Reviewed)
                return Json(new { success = false, message = "This task has been reviewed and is locked." });

            var ps = assignment.PointStatuses.FirstOrDefault(p => p.Id == pointStatusId);
            if (ps is null)
                return Json(new { success = false, message = "Point not found." });

            ps.IsCompleted = !ps.IsCompleted;
            ps.CompletedAt = ps.IsCompleted ? DateTime.UtcNow : null;

            return await SaveProgressAsync(task!, assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Tasks.UpdateProgress)]
        public async Task<IActionResult> UpdateCounter(int workTaskId, int value)
        {
            var (task, assignment) = await LoadOwnedAsync(workTaskId);
            if (assignment is null)
                return Json(new { success = false, message = "Not authorized for this task." });

            if (assignment.Status == WorkTaskStatus.Reviewed)
                return Json(new { success = false, message = "This task has been reviewed and is locked." });

            var target = task!.TargetCount ?? 0;
            assignment.CompletedCount = Math.Clamp(value, 0, target);

            return await SaveProgressAsync(task, assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Tasks.UpdateProgress)]
        public async Task<IActionResult> SetStatus(int workTaskId, WorkTaskStatus status)
        {
            var (task, assignment) = await LoadOwnedAsync(workTaskId);
            if (assignment is null)
                return Json(new { success = false, message = "Not authorized for this task." });

            if (assignment.Status == WorkTaskStatus.Reviewed)
                return Json(new { success = false, message = "This task has been reviewed and is locked." });

            // Explicit status only applies to Normal tasks; others are auto-computed.
            if (task!.TaskType.Category != TaskCategory.Normal)
                return Json(new { success = false, message = "Status is computed automatically for this task type." });

            if (!Enum.IsDefined(status))
                return Json(new { success = false, message = "Invalid status." });

            assignment.Status = status;

            return await SaveProgressAsync(task, assignment, recomputeAssignment: false);
        }

        // ─── Admin: review sign-off ──────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Tasks.Review)]
        public async Task<IActionResult> MarkAssignmentReviewed(int workTaskId, int assignmentId)
        {
            var task = await _unitOfWork.Repository<WorkTask>().GetByIdAsync(new WorkTaskSpec(workTaskId));
            if (task is null)
                return Json(new { success = false, message = "Task not found." });

            var assignment = task.Assignments.FirstOrDefault(a => a.Id == assignmentId);
            if (assignment is null)
                return Json(new { success = false, message = "Assignment not found." });

            if (assignment.Status != WorkTaskStatus.Completed)
                return Json(new { success = false, message = "Only completed work can be reviewed." });

            assignment.Status = WorkTaskStatus.Reviewed;
            RecomputeTaskStatus(task);

            _unitOfWork.Repository<WorkTask>().Update(task);
            await _unitOfWork.CompleteAsync();

            return Json(new
            {
                success = true,
                message = $"{assignment.User?.FullName ?? "Employee"}'s work has been marked as reviewed.",
                assignmentStatus = assignment.Status.ToString(),
                taskStatus = task.Status.ToString()
            });
        }

        // ─── Comments (admin creator + assignees) ────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Permissions.Tasks.Comment)]
        public async Task<IActionResult> AddComment(int workTaskId, int taskAssignmentId, string? content, IFormFile? file, string? returnUrl)
        {
            var task = await _unitOfWork.Repository<WorkTask>().GetByIdAsync(new WorkTaskSpec(workTaskId));
            var safeReturn = Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Action("Index")!;

            if (task is null)
            {
                TempData["Error"] = "Task not found.";
                return Redirect(safeReturn);
            }

            var userId = _userManager.GetUserId(User)!;

            // Validate the assignment belongs to this task
            var targetAssignment = task.Assignments.FirstOrDefault(a => a.Id == taskAssignmentId);
            if (targetAssignment is null)
            {
                TempData["Error"] = "Invalid thread.";
                return Redirect(safeReturn);
            }

            // Admin (creator) may post to any thread; employee may only post to their own
            var isCreator = task.CreatedByUserId == userId;
            var isOwnAssignment = targetAssignment.UserId == userId;
            if (!isCreator && !isOwnAssignment)
            {
                TempData["Error"] = "Not authorized for this thread.";
                return Redirect(safeReturn);
            }

            var comment = new TaskComment
            {
                WorkTaskId = workTaskId,
                TaskAssignmentId = taskAssignmentId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            if (file is not null && file.Length > 0)
            {
                var fileName = DocumentSettings.UplaodFile(file, CommentsFolder);
                comment.FileUrl = fileName;
                comment.Type = IsImage(file.FileName) ? CommentType.Image : CommentType.File;
            }
            else if (!string.IsNullOrWhiteSpace(content))
            {
                comment.Content = content.Trim();
                comment.Type = CommentType.Text;
            }
            else
            {
                TempData["Error"] = "Cannot send an empty message.";
                return Redirect(safeReturn);
            }

            await _unitOfWork.Repository<TaskComment>().AddAsync(comment);
            await _unitOfWork.CompleteAsync();

            return Redirect(safeReturn);
        }

        // ─── Cascading dropdown / picker AJAX ────────────────────────────────

        [HttpGet]
        [Authorize(Policy = Permissions.Tasks.Create)]
        public async Task<IActionResult> GetSectionsByCorporation(int corporationId)
        {
            var sections = await _unitOfWork.Repository<Section>().GetAllAsync(new SectionByCorporationSpec(corporationId));
            return Json(sections.Select(s => new { value = s.Id, text = s.Name }));
        }

        [HttpGet]
        [Authorize(Policy = Permissions.Tasks.Create)]
        public async Task<IActionResult> GetAvailableEmployees(int corporationId, int? sectionId)
        {
            var employees = await GetAssignableEmployeesAsync(corporationId, sectionId);
            return Json(employees.Select(u => new AvailableEmployeeViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email
            }));
        }

        // ─── Private helpers ─────────────────────────────────────────────────

        private async Task<(WorkTask? task, TaskAssignment? assignment)> LoadOwnedAsync(int workTaskId)
        {
            var userId = _userManager.GetUserId(User)!;
            var task = await _unitOfWork.Repository<WorkTask>().GetByIdAsync(new WorkTaskSpec(workTaskId));
            var assignment = task?.Assignments.FirstOrDefault(a => a.UserId == userId);
            return (task, assignment);
        }

        private async Task<IActionResult> SaveProgressAsync(WorkTask task, TaskAssignment assignment, bool recomputeAssignment = true)
        {
            if (recomputeAssignment)
                RecomputeAssignmentStatus(assignment, task);
            RecomputeTaskStatus(task);

            _unitOfWork.Repository<WorkTask>().Update(task);
            await _unitOfWork.CompleteAsync();

            return Json(new
            {
                success = true,
                assignmentStatus = assignment.Status.ToString(),
                taskStatus = task.Status.ToString(),
                progressPercent = ProgressPercent(task.TaskType.Category, assignment, task)
            });
        }

        private async Task<List<AppUser>> GetAssignableEmployeesAsync(int corporationId, int? sectionId)
        {
            var employees = await _userManager.GetUsersInRoleAsync(Roles.Employee);
            return employees
                .Where(u => u.IsActive
                         && u.CorporationId == corporationId
                         && (sectionId is null || u.SectionId == sectionId))
                .OrderBy(u => u.FirstName)
                .ToList();
        }

        private async Task<List<string>> ResolveAssigneeIdsAsync(int corporationId, int? sectionId, List<string> selectedUserIds)
        {
            var assignable = await GetAssignableEmployeesAsync(corporationId, sectionId);
            var allowed = assignable.Select(u => u.Id).ToHashSet();
            return selectedUserIds.Where(id => allowed.Contains(id)).Distinct().ToList();
        }

        private async Task ReconcilePointsAsync(WorkTask task, WorkTaskViewModel model, TaskCategory category)
        {
            if (category != TaskCategory.Point)
                return;

            var submitted = model.Points.Where(p => !string.IsNullOrWhiteSpace(p.Description)).ToList();
            var submittedIds = submitted.Where(p => p.Id != 0).Select(p => p.Id).ToHashSet();

            // Remove points the admin dropped (clear their statuses first — NoAction FK)
            foreach (var point in task.Points.Where(p => !submittedIds.Contains(p.Id)).ToList())
            {
                foreach (var ps in task.Assignments.SelectMany(a => a.PointStatuses).Where(ps => ps.TaskPointId == point.Id).ToList())
                    _unitOfWork.Repository<TaskPointStatus>().Delete(ps);
                _unitOfWork.Repository<TaskPoint>().Delete(point);
            }
            await _unitOfWork.CompleteAsync();

            // Update existing + add new (re-number Order by submitted sequence)
            var order = 1;
            var newPoints = new List<TaskPoint>();
            foreach (var p in submitted)
            {
                if (p.Id != 0)
                {
                    var existing = task.Points.FirstOrDefault(x => x.Id == p.Id);
                    if (existing is not null)
                    {
                        existing.Description = p.Description.Trim();
                        existing.Order = order;
                        _unitOfWork.Repository<TaskPoint>().Update(existing);
                    }
                }
                else
                {
                    var point = new TaskPoint { Description = p.Description.Trim(), Order = order, WorkTaskId = task.Id };
                    await _unitOfWork.Repository<TaskPoint>().AddAsync(point);
                    newPoints.Add(point);
                }
                order++;
            }
            await _unitOfWork.CompleteAsync();

            // Seed statuses for newly added points across all current assignments
            foreach (var point in newPoints)
                foreach (var assignment in task.Assignments)
                    await _unitOfWork.Repository<TaskPointStatus>().AddAsync(new TaskPointStatus
                    {
                        TaskAssignmentId = assignment.Id,
                        TaskPointId = point.Id,
                        IsCompleted = false
                    });
            await _unitOfWork.CompleteAsync();
        }

        private async Task ReconcileAssignmentsAsync(WorkTask task, List<string> employeeIds, TaskCategory category)
        {
            var target = employeeIds.ToHashSet();

            // Remove assignees no longer selected (clear point statuses first — NoAction FK)
            foreach (var assignment in task.Assignments.Where(a => !target.Contains(a.UserId)).ToList())
            {
                foreach (var ps in assignment.PointStatuses.ToList())
                    _unitOfWork.Repository<TaskPointStatus>().Delete(ps);
                _unitOfWork.Repository<TaskAssignment>().Delete(assignment);
            }
            await _unitOfWork.CompleteAsync();

            // Add newly selected assignees
            var current = task.Assignments.Select(a => a.UserId).ToHashSet();
            var points = await _unitOfWork.Repository<TaskPoint>().GetAllAsync(new WorkTaskPointsByTaskSpecLocal(task.Id));
            foreach (var userId in employeeIds.Where(id => !current.Contains(id)))
            {
                var assignment = new TaskAssignment
                {
                    WorkTaskId = task.Id,
                    UserId = userId,
                    Status = WorkTaskStatus.Pending,
                    AssignedAt = DateTime.UtcNow,
                    CompletedCount = category == TaskCategory.Counter ? 0 : null
                };
                await _unitOfWork.Repository<TaskAssignment>().AddAsync(assignment);
                await _unitOfWork.CompleteAsync();

                foreach (var point in points)
                    await _unitOfWork.Repository<TaskPointStatus>().AddAsync(new TaskPointStatus
                    {
                        TaskAssignmentId = assignment.Id,
                        TaskPointId = point.Id,
                        IsCompleted = false
                    });
            }
            await _unitOfWork.CompleteAsync();
        }

        private void RecomputeAllStatuses(WorkTask task)
        {
            foreach (var assignment in task.Assignments)
                RecomputeAssignmentStatus(assignment, task);
            RecomputeTaskStatus(task);
        }

        private void RecomputeAssignmentStatus(TaskAssignment assignment, WorkTask task)
        {
            // Reviewed is a final state set by the admin — never recompute over it.
            if (assignment.Status == WorkTaskStatus.Reviewed)
                return;

            var category = task.TaskType.Category;
            if (category == TaskCategory.Point)
            {
                var total = task.Points.Count;
                var done = assignment.PointStatuses.Count(ps => ps.IsCompleted);
                assignment.Status = total > 0 && done == total ? WorkTaskStatus.Completed
                                  : done > 0 ? WorkTaskStatus.InProgress
                                  : WorkTaskStatus.Pending;
            }
            else if (category == TaskCategory.Counter)
            {
                var target = task.TargetCount ?? 0;
                var done = assignment.CompletedCount ?? 0;
                assignment.Status = target > 0 && done >= target ? WorkTaskStatus.Completed
                                  : done > 0 ? WorkTaskStatus.InProgress
                                  : WorkTaskStatus.Pending;
            }
            // Normal: status is set explicitly via SetStatus — leave as-is
        }

        private void RecomputeTaskStatus(WorkTask task)
        {
            if (task.Assignments.Count == 0)
            {
                task.Status = WorkTaskStatus.Pending;
                return;
            }
            if (task.Assignments.All(a => a.Status == WorkTaskStatus.Reviewed))
                task.Status = WorkTaskStatus.Reviewed;
            else if (task.Assignments.All(a => a.Status is WorkTaskStatus.Completed or WorkTaskStatus.Reviewed))
                task.Status = WorkTaskStatus.Completed;
            else if (task.Assignments.Any(a => a.Status != WorkTaskStatus.Pending))
                task.Status = WorkTaskStatus.InProgress;
            else
                task.Status = WorkTaskStatus.Pending;
        }

        private static int ProgressPercent(TaskCategory category, TaskAssignment assignment, WorkTask task)
        {
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
                    return assignment.Status switch
                    {
                        WorkTaskStatus.Completed  => 100,
                        WorkTaskStatus.InProgress => 50,
                        _ => 0
                    };
            }
        }

        private List<AssignmentProgressViewModel> BuildAssignmentProgress(WorkTask task)
        {
            var category = task.TaskType.Category;
            return task.Assignments.Select(a => new AssignmentProgressViewModel
            {
                AssignmentId = a.Id,
                UserName = a.User?.FullName,
                Status = a.Status,
                ProgressPercent = ProgressPercent(category, a, task),
                CompletedCount = a.CompletedCount,
                TargetCount = task.TargetCount,
                PointsDone = a.PointStatuses.Count(ps => ps.IsCompleted),
                PointsTotal = task.Points.Count
            }).ToList();
        }

        private List<TaskCommentViewModel> BuildComments(IEnumerable<TaskComment> comments)
        {
            var userId = _userManager.GetUserId(User);
            return comments.Select(c =>
            {
                var vm = _mapper.Map<TaskComment, TaskCommentViewModel>(c);
                vm.IsMine = c.UserId == userId;
                if (vm.Type != CommentType.Text && !string.IsNullOrEmpty(vm.FileUrl))
                    vm.FileUrl = $"/Files/{CommentsFolder}/{vm.FileUrl}";
                return vm;
            }).ToList();
        }

        private async Task<List<TaskCommentViewModel>> BuildCommentsForAssignmentAsync(int assignmentId)
        {
            var comments = await _unitOfWork.Repository<TaskComment>()
                .GetAllAsync(new TaskCommentByAssignmentSpec(assignmentId));
            return BuildComments(comments);
        }

        private async Task<TaskCategory> ResolveCategoryAsync(int taskTypeId)
        {
            if (taskTypeId <= 0) return TaskCategory.Normal;
            var type = await _unitOfWork.Repository<TaskType>().GetByIdAsync(new TaskTypeSpec(taskTypeId));
            return type?.Category ?? TaskCategory.Normal;
        }

        private void ValidateByCategory(WorkTaskViewModel model, TaskCategory category)
        {
            if (category == TaskCategory.Counter && (model.TargetCount is null || model.TargetCount < 1))
                ModelState.AddModelError(nameof(model.TargetCount), "Target count is required for counter tasks.");

            if (category == TaskCategory.Point && !model.Points.Any(p => !string.IsNullOrWhiteSpace(p.Description)))
                ModelState.AddModelError(nameof(model.Points), "Add at least one point for a point-type task.");

            if (model.DueDate.Date < model.RequestDate.Date)
                ModelState.AddModelError(nameof(model.DueDate), "Due date cannot be before the request date.");
        }

        private async Task PopulateLookupsAsync(WorkTaskViewModel model)
        {
            var taskTypes = await _unitOfWork.Repository<TaskType>().GetAllAsync(new TaskTypeSpec());
            model.TaskTypes = taskTypes.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name });
            model.TaskTypeCategoryMap = taskTypes.ToDictionary(t => t.Id, t => (int)t.Category);

            var corporations = await _unitOfWork.Repository<Corporation>().GetAllAsync(new CorporationSpec());
            model.Corporations = corporations.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });

            if (model.CorporationId > 0)
            {
                var sections = await _unitOfWork.Repository<Section>().GetAllAsync(new SectionByCorporationSpec(model.CorporationId));
                model.Sections = sections.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });

                var employees = await GetAssignableEmployeesAsync(model.CorporationId, model.SectionId);
                model.AvailableEmployees = employees.Select(u => new AvailableEmployeeViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    IsSelected = model.SelectedUserIds.Contains(u.Id)
                });
            }
        }

        private static bool IsImage(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp";
        }
    }

    // Local spec to fetch a task's points without circular include weight.
    internal class WorkTaskPointsByTaskSpecLocal : Tasks.Domain.Specifications.BaseSpecifications<TaskPoint>
    {
        public WorkTaskPointsByTaskSpecLocal(int workTaskId) : base(p => p.WorkTaskId == workTaskId) { }
    }
}

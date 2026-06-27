using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Tasks.Domain.Enums;

namespace Tasks.Presentation.ViewModels
{
    public class WorkTaskViewModel
    {
        public int Id { get; set; }

        public string? Code { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        [MinLength(2, ErrorMessage = "Title must be at least 2 characters.")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(5000, ErrorMessage = "Description cannot exceed 5000 characters.")]
        public string? Description { get; set; }

        [MaxLength(5000, ErrorMessage = "Notes cannot exceed 5000 characters.")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Request date is required.")]
        [DataType(DataType.Date)]
        public DateTime RequestDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Due date is required.")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);

        [Required(ErrorMessage = "Priority is required.")]
        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

        [Required(ErrorMessage = "Task type is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a task type.")]
        public int TaskTypeId { get; set; }

        [Required(ErrorMessage = "Corporation is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a corporation.")]
        public int CorporationId { get; set; }

        public int? SectionId { get; set; }

        // Counter-type tasks only.
        [Range(1, int.MaxValue, ErrorMessage = "Target count must be at least 1.")]
        public int? TargetCount { get; set; }

        // Assignees + checklist points captured from the form.
        public List<string> SelectedUserIds { get; set; } = new();
        public List<TaskPointViewModel> Points { get; set; } = new();

        // ─── Display / helper fields (not bound from the create form) ────────────
        public WorkTaskStatus Status { get; set; }
        public string? TaskTypeName { get; set; }
        public TaskCategory TaskCategory { get; set; }
        public string? CorporationName { get; set; }
        public string? SectionName { get; set; }
        public string? CreatedByName { get; set; }
        public int AssigneeCount { get; set; }

        // Select lists & lookups.
        public IEnumerable<SelectListItem> TaskTypes { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Corporations { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Sections { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<AvailableEmployeeViewModel> AvailableEmployees { get; set; } = Enumerable.Empty<AvailableEmployeeViewModel>();

        // Maps TaskTypeId -> TaskCategory (int) so the form can toggle Counter/Point sections.
        public Dictionary<int, int> TaskTypeCategoryMap { get; set; } = new();
    }
}

using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Tasks.Domain.Enums;

namespace Tasks.Presentation.ViewModels
{
    public class TaskTypeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters.")]
        [Remote(action: "CheckUniqueName", controller: "TaskType",
                AdditionalFields = nameof(Id),
                ErrorMessage = "This task type name already exists.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        public TaskCategory Category { get; set; }
    }
}

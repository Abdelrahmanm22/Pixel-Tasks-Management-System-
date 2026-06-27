using System.ComponentModel.DataAnnotations;

namespace Tasks.Presentation.ViewModels
{
    public class TaskPointViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Point description is required.")]
        [MaxLength(500, ErrorMessage = "Point description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        public int Order { get; set; }
    }
}

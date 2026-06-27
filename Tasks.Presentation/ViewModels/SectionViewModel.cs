using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Tasks.Presentation.ViewModels
{
    public class SectionViewModel
    {
        public int Id { get; set; }

        public string? Code { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters.")]
        [Remote(action: "CheckUniqueName", controller: "Section",
                AdditionalFields = nameof(Id),
                ErrorMessage = "This section name already exists.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Corporation is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a corporation.")]
        public int CorporationId { get; set; }

        [MaxLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string? Email { get; set; }

        [MaxLength(50, ErrorMessage = "Fax cannot exceed 50 characters.")]
        public string? Fax { get; set; }

        [MaxLength(50, ErrorMessage = "Phone cannot exceed 50 characters.")]
        public string? Phone { get; set; }

        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public string? Address { get; set; }

        [MaxLength(50, ErrorMessage = "Telex cannot exceed 50 characters.")]
        public string? Telex { get; set; }

        [MaxLength(5000, ErrorMessage = "Notes cannot exceed 5000 characters.")]
        public string? Notes { get; set; }

        // Employees to assign
        public List<string> SelectedUserIds { get; set; } = new();

        // Display helpers (not mapped from form)
        public string? CorporationName { get; set; }
        public int MemberCount { get; set; }
        public IEnumerable<SelectListItem> Corporations { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<AvailableEmployeeViewModel> AvailableEmployees { get; set; } = Enumerable.Empty<AvailableEmployeeViewModel>();
    }
}

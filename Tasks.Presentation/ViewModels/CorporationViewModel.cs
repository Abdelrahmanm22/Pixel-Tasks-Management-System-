using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Tasks.Presentation.ViewModels
{
    public class CorporationViewModel
    {
        public int Id { get; set; }

        /// <summary>Auto-generated code (e.g. PXC-000001). Read-only on the form.</summary>
        public string? Code { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters.")]
        [Remote(action: "CheckUniqueName", controller: "Corporation",
                AdditionalFields = nameof(Id),
                ErrorMessage = "This corporation name already exists.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "Arabic name cannot exceed 200 characters.")]
        public string? NameAr { get; set; }

        // Notes stays in the model (and database) — just hidden from the list view
        [MaxLength(5000, ErrorMessage = "Notes cannot exceed 5000 characters.")]
        public string? Notes { get; set; }
    }
}

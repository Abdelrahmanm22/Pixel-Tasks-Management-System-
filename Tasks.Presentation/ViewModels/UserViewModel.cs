using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Tasks.Domain.Enums;

namespace Tasks.Presentation.ViewModels
{
    public class UserViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        [Display(Name = "Username")]
        [Remote(action: "CheckUniqueUserName", controller: "User", AdditionalFields = nameof(Id),
                HttpMethod = "GET", ErrorMessage = "This username is already taken.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [MaxLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
        [Display(Name = "Email")]
        [Remote(action: "CheckUniqueEmail", controller: "User", AdditionalFields = nameof(Id),
                HttpMethod = "GET", ErrorMessage = "This email is already taken.")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
        [Display(Name = "Phone")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [Display(Name = "Gender")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Corporation")]
        public int? CorporationId { get; set; }

        [Display(Name = "Section")]
        public int? SectionId { get; set; }

        public bool IsActive { get; set; } = true;

        // Create-only: password fields
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Profile Image")]
        public IFormFile? ProfileImage { get; set; }

        public string? ExistingImageUrl { get; set; }

        // Display helpers (not mapped from form)
        public string? FullName => $"{FirstName} {LastName}".Trim();
        public string? CorporationName { get; set; }
        public string? SectionName { get; set; }

        public IEnumerable<SelectListItem> Corporations { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Sections { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Roles { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}

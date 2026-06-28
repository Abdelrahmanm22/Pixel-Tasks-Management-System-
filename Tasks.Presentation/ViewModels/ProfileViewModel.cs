using Microsoft.AspNetCore.Http;
using Tasks.Domain.Enums;

namespace Tasks.Presentation.ViewModels
{
    public class ProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public Gender Gender { get; set; }
        public string? ImageUrl { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? CorporationName { get; set; }
        public string? SectionName { get; set; }

        public IFormFile? ProfileImage { get; set; }
    }
}

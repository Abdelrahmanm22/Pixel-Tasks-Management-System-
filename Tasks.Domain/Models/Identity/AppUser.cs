using Microsoft.AspNetCore.Identity;
using Tasks.Domain.Enums;

namespace Tasks.Domain.Models.Identity
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        
        public string FullName => $"{FirstName} {LastName}".Trim();

        public Gender Gender { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>Optional profile image URL.</summary>
        public string? ImageUrl { get; set; }
    }
}

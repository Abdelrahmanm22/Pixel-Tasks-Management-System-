using Microsoft.AspNetCore.Identity;
using Tasks.Domain.Enums;

namespace Tasks.Domain.Models.Identity
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName  { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}".Trim();

        public Gender Gender  { get; set; }
        public bool IsActive  { get; set; } = true;

        /// <summary>Optional profile image URL.</summary>
        public string? ImageUrl { get; set; }

        // FKs — both nullable:
        //   null CorporationId → admin / task creator not tied to any corporation
        //   null SectionId     → user belongs to a corporation but has no section
        public int? CorporationId { get; set; }
        public int? SectionId     { get; set; }

        // Navigation
        public Corporation? Corporation { get; set; }
        public Section? Section         { get; set; }
    }
}

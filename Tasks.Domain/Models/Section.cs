using Tasks.Domain.Models.Identity;

namespace Tasks.Domain.Models
{
    public class Section : BaseModel, ICodedEntity
    {
        public string Name    { get; set; } = string.Empty;
        public string Code    { get; set; } = string.Empty;
        public string? Email  { get; set; }
        public string? Fax    { get; set; }
        public string? Phone  { get; set; }
        public string? Address { get; set; }
        public string? Telex  { get; set; }
        public string? Notes  { get; set; }

        // FK
        public int CorporationId { get; set; }

        // Navigation
        public Corporation Corporation { get; set; } = null!;
        public ICollection<AppUser> Users { get; set; } = new HashSet<AppUser>();
    }
}

using Tasks.Domain.Models.Identity;

namespace Tasks.Domain.Models
{
    public class Corporation : BaseModel, ICodedEntity
    {
        public string Name    { get; set; } = string.Empty;
        public string? NameAr { get; set; }
        public string Code    { get; set; } = string.Empty;
        public string? Notes  { get; set; }

        // Navigation
        public ICollection<Section> Sections { get; set; } = new HashSet<Section>();
        public ICollection<AppUser> Users    { get; set; } = new HashSet<AppUser>();
    }
}

using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.CorporationSpec
{
    /// <summary>
    /// Finds a corporation by its exact name (case-insensitive).
    /// Used for remote unique-name validation.
    /// </summary>
    public class CorporationByNameSpec : BaseSpecifications<Corporation>
    {
        public CorporationByNameSpec(string name)
            : base(c => c.Name.ToLower() == name.ToLower())
        { }
    }
}

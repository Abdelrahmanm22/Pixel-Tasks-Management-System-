using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.SectionSpec
{
    public class SectionByNameSpec : BaseSpecifications<Section>
    {
        public SectionByNameSpec(string name)
            : base(s => s.Name.ToLower() == name.ToLower())
        { }
    }
}

using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.SectionSpec
{
    public class SectionSpec : BaseSpecifications<Section>
    {
        public SectionSpec() : base()
        {
            AddInclude("Corporation");
            AddInclude("Users");
        }

        public SectionSpec(int id) : base(s => s.Id == id)
        {
            AddInclude("Corporation");
            AddInclude("Users");
        }
    }
}

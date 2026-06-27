using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.SectionSpec
{
    public class SectionByCorporationSpec : BaseSpecifications<Section>
    {
        public SectionByCorporationSpec(int corporationId)
            : base(s => s.CorporationId == corporationId)
        {
            SetOrderBy(s => s.Name);
        }
    }
}

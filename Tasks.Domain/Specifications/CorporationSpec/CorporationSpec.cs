using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.CorporationSpec
{
    public class CorporationSpec : BaseSpecifications<Corporation>
    {

        public CorporationSpec() : base()
        { }

        //Get By Id
        public CorporationSpec(int id) : base(c => c.Id == id)
        { }
    }
}

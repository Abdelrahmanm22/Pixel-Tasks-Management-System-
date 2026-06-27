using AutoMapper;
using Tasks.Domain.Models;
using Tasks.Presentation.ViewModels;

namespace Tasks.Presentation.MappingProfiles
{
    public class CorporationProfile : Profile
    {
        public CorporationProfile()
        {
            // Domain → ViewModel (for display)
            CreateMap<Corporation, CorporationViewModel>();

            // ViewModel → Domain (for Create / Edit)
            CreateMap<CorporationViewModel, Corporation>()
                .ForMember(dest => dest.Code, opt => opt.Ignore()); // Code is auto-generated, not mapped from VM
        }
    }
}

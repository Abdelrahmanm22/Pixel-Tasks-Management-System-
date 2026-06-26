using AutoMapper;
using Tasks.Domain.Models;
using Tasks.Presentation.ViewModels;

namespace Tasks.Presentation.MappingProfiles
{
    public class CorporationProfile : Profile
    {
        public CorporationProfile()
        {
            CreateMap<Corporation, CorporationViewModel>();
        }
    }
}

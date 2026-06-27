using AutoMapper;
using Tasks.Domain.Models;
using Tasks.Presentation.ViewModels;

namespace Tasks.Presentation.MappingProfiles
{
    public class SectionProfile : Profile
    {
        public SectionProfile()
        {
            CreateMap<Section, SectionViewModel>()
                .ForMember(d => d.CorporationName,   opt => opt.MapFrom(s => s.Corporation != null ? s.Corporation.Name : null))
                .ForMember(d => d.MemberCount,       opt => opt.MapFrom(s => s.Users != null ? s.Users.Count : 0))
                .ForMember(d => d.Corporations,      opt => opt.Ignore())
                .ForMember(d => d.AvailableEmployees, opt => opt.Ignore())
                .ForMember(d => d.SelectedUserIds,   opt => opt.Ignore());

            CreateMap<SectionViewModel, Section>()
                .ForMember(d => d.Code,          opt => opt.Ignore())
                .ForMember(d => d.Corporation,   opt => opt.Ignore())
                .ForMember(d => d.Users,         opt => opt.Ignore());
        }
    }
}

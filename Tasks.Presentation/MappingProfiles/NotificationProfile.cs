using AutoMapper;
using Tasks.Domain.Models;
using Tasks.Presentation.ViewModels;

namespace Tasks.Presentation.MappingProfiles
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            CreateMap<Notification, NotificationViewModel>()
                .ForMember(d => d.ActorName,     opt => opt.MapFrom(s => s.Actor != null ? s.Actor.FullName : null))
                .ForMember(d => d.ActorImageUrl, opt => opt.MapFrom(s => s.Actor != null ? s.Actor.ImageUrl : null))
                .ForMember(d => d.ActorGender,   opt => opt.MapFrom(s => s.Actor != null ? s.Actor.Gender : default));
        }
    }
}

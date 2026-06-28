using AutoMapper;
using Tasks.Domain.Models;
using Tasks.Presentation.ViewModels;

namespace Tasks.Presentation.MappingProfiles
{
    public class WorkTaskProfile : Profile
    {
        public WorkTaskProfile()
        {
            CreateMap<WorkTask, WorkTaskViewModel>()
                .ForMember(d => d.TaskTypeName,    opt => opt.MapFrom(s => s.TaskType != null ? s.TaskType.Name : null))
                .ForMember(d => d.TaskCategory,    opt => opt.MapFrom(s => s.TaskType != null ? s.TaskType.Category : default))
                .ForMember(d => d.CorporationName, opt => opt.MapFrom(s => s.Corporation != null ? s.Corporation.Name : null))
                .ForMember(d => d.SectionName,     opt => opt.MapFrom(s => s.Section != null ? s.Section.Name : null))
                .ForMember(d => d.CreatedByName,   opt => opt.MapFrom(s => s.CreatedBy != null ? s.CreatedBy.FullName : null))
                .ForMember(d => d.AssigneeCount,   opt => opt.MapFrom(s => s.Assignments != null ? s.Assignments.Count : 0))
                .ForMember(d => d.SelectedUserIds, opt => opt.Ignore())
                .ForMember(d => d.Points,          opt => opt.Ignore())
                .ForMember(d => d.TaskTypes,       opt => opt.Ignore())
                .ForMember(d => d.Corporations,    opt => opt.Ignore())
                .ForMember(d => d.Sections,        opt => opt.Ignore())
                .ForMember(d => d.AvailableEmployees,  opt => opt.Ignore())
                .ForMember(d => d.TaskTypeCategoryMap, opt => opt.Ignore());

            CreateMap<WorkTaskViewModel, WorkTask>()
                .ForMember(d => d.Code,            opt => opt.Ignore())
                .ForMember(d => d.Status,          opt => opt.Ignore())
                .ForMember(d => d.CreatedByUserId, opt => opt.Ignore())
                .ForMember(d => d.TaskType,        opt => opt.Ignore())
                .ForMember(d => d.CreatedBy,       opt => opt.Ignore())
                .ForMember(d => d.Corporation,     opt => opt.Ignore())
                .ForMember(d => d.Section,         opt => opt.Ignore())
                .ForMember(d => d.Assignments,     opt => opt.Ignore())
                .ForMember(d => d.Points,          opt => opt.Ignore())
                .ForMember(d => d.Comments,        opt => opt.Ignore());

            CreateMap<TaskComment, TaskCommentViewModel>()
                .ForMember(d => d.UserName,     opt => opt.MapFrom(s => s.User != null ? s.User.FullName : null))
                .ForMember(d => d.UserImageUrl, opt => opt.MapFrom(s => s.User != null ? s.User.ImageUrl : null))
                .ForMember(d => d.UserGender,   opt => opt.MapFrom(s => s.User != null ? s.User.Gender : default))
                .ForMember(d => d.IsMine,       opt => opt.Ignore());
        }
    }
}

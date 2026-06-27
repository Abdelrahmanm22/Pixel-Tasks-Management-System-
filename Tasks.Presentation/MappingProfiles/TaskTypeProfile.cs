using AutoMapper;
using Tasks.Domain.Models;
using Tasks.Presentation.ViewModels;

namespace Tasks.Presentation.MappingProfiles
{
    public class TaskTypeProfile : Profile
    {
        public TaskTypeProfile()
        {
            CreateMap<TaskType, TaskTypeViewModel>();
            CreateMap<TaskTypeViewModel, TaskType>();
        }
    }
}

using AutoMapper;
using ProjectManager.Models;
using ProjectManager.Dto.Task;
using ProjectManager.Dto.Auth;
using ProjectManager.Dto.Project;
using ProjectManager.Dto.Users;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ProjectManager.Models.Task, TaskDto>().ReverseMap();
        CreateMap<User, UserResponseDto>()
        .ForMember(dest => dest.Roles, opt => opt.Ignore()); 

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore());

        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.Manager, opt => opt.MapFrom(src => src.Manager))
            .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.ProjectUsers.Select(pu => pu.User).ToList()))
            .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.Tasks.Count));

        CreateMap<CreateProjectDto, Project>();
        CreateMap<UpdateProjectDto, Project>();
    }
}
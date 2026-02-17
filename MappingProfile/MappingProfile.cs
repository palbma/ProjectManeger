using AutoMapper;
using ProjectManager.Models;
using ProjectManager.Dto.Task;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ProjectManager.Models.Task, TaskDto>().ReverseMap();
    }
}
using ProjectManager.Dto.Project;

namespace ProjectManager.Services.Interfaces
{
    public interface IProjectService
    {
        Task<List<ProjectDto>> GetAllProjects(int userId, string role);
        Task<ProjectDto> GetProjectById(int projectId, int userId, string role);
        Task<ProjectDto> CreateProject(CreateProjectDto createProjectDto, int managerId);
        Task UpdateProject(int projectId, UpdateProjectDto updateProjectDto, int userId, string role);
        Task DeleteProject(int projectId, int userId, string role);
        Task AddMember(int projectId, int memberId, int currentUserId);
        Task RemoveMember(int projectId, int memberId, int currentUserId);
    }
}

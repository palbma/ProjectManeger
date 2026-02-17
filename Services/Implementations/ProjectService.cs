using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Dto.Project;
using ProjectManager.Models;
using ProjectManager.Models.Enums;
using ProjectManager.Services.Interfaces;
using System;
using Task = ProjectManager.Models.Task;

namespace ProjectManager.Services.Implementations
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProjectService(ApplicationDbContext applicationDbContext, IMapper mapper)
        {
            _context = applicationDbContext;
            _mapper = mapper;
        }

        System.Threading.Tasks.Task IProjectService.AddMember(int projectId, int memberId, int currentUserId)
        {
            throw new NotImplementedException();
        }

        async Task<ProjectDto> IProjectService.CreateProject(CreateProjectDto createProjectDto, int managerId)
        {
            if (createProjectDto.Deadline <= DateTime.UtcNow)
                throw new NotImplementedException("Дедлайн должен быть в будущем");

            var project = new Project
            {
                Title = createProjectDto.Title,
                Description = createProjectDto.Description,
                Deadline = createProjectDto.Deadline,
                ManagerId = managerId,
                Status = ProjectStatus.Active,
                StartDate = DateTime.UtcNow
            };

            if (createProjectDto.MemberIds != null && createProjectDto.MemberIds.Any())
            {
                var memberIds = createProjectDto.MemberIds
                    .Distinct()
                    .Where(id => id != managerId)
                    .ToList();

                var users = await _context.Users
                    .Where(u => memberIds.Contains(u.Id))
                    .ToListAsync();

                project.ProjectUsers = users
                    .Select(u => new ProjectUser { User = u })
                    .ToList();
            }

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return await GetProjectById(project.Id, managerId, "Admin");
        }
        public async Task<ProjectDto> GetProjectById(int projectId, int userId, string role)
        {
            var project = await _context.Projects
                .Include(p => p.Manager)
                .Include(p => p.ProjectUsers)
                    .ThenInclude(pu => pu.User)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new NotImplementedException($"Project  {projectId} Not Found");

            bool hasAccess = role == "Admin"
                || project.ManagerId == userId
                || project.ProjectUsers.Any(pu => pu.UserId == userId);

            if (!hasAccess)
                throw new NotImplementedException("No acces to project");

            return _mapper.Map<ProjectDto>(project);
        }
        System.Threading.Tasks.Task IProjectService.DeleteProject(int projectId, int userId, string role)
        {
            throw new NotImplementedException();
        }

        Task<List<ProjectDto>> IProjectService.GetAllProjects(int userId, string role)
        {
            throw new NotImplementedException();
        }

        Task<ProjectDto> IProjectService.GetProjectById(int projectId, int userId, string role)
        {
            throw new NotImplementedException();
        }

        System.Threading.Tasks.Task IProjectService.RemoveMember(int projectId, int memberId, int currentUserId)
        {
            throw new NotImplementedException();
        }

        System.Threading.Tasks.Task IProjectService.UpdateProject(int projectId, UpdateProjectDto updateProjectDto, int userId, string role)
        {
            throw new NotImplementedException();
        }
    }
}

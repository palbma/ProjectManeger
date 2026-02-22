using AutoMapper;
using Microsoft.AspNetCore.Mvc;
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
        [HttpPost("{projectId}/members/{memberId}")]
        public async System.Threading.Tasks.Task AddMember(int projectId, int memberId, int currentUserId)
        {
            var currentUserRole = await GetUserRole(currentUserId);
            var project = await _context.Projects
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new Exception($"Проект с ID {projectId} не найден");

            if (currentUserRole != "Admin" && project.ManagerId != currentUserId)
                throw new Exception("Вы не можете добавлять участников");

            var user = await _context.Users.FindAsync(memberId);
            if (user == null)
                throw new Exception($"Пользователь с ID {memberId} не найден");

            if (project.ManagerId == memberId)
                throw new Exception("Менеджер уже является участником");

            if (project.ProjectUsers.Any(pu => pu.UserId == memberId))
                throw new Exception("Пользователь уже участник");

            project.ProjectUsers.Add(new ProjectUser
            {
                ProjectId = project.Id,
                UserId = memberId,
                JoinedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        [HttpPost]
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
        public async Task<List<ProjectDto>> GetAllProjects(int userId, string role)
        {
            var query = _context.Projects
                .Include(p => p.Manager)
                .Include(p => p.ProjectUsers)
                    .ThenInclude(pu => pu.User)
                .Include(p => p.Tasks)
                .AsQueryable();

            if (role != "Admin")
            {
                if (role == "ProjectManager")
                    query = query.Where(p => p.ManagerId == userId);
                else 
                    query = query.Where(p => p.ProjectUsers.Any(pu => pu.UserId == userId));
            }

            var projects = await query.ToListAsync();
            return _mapper.Map<List<ProjectDto>>(projects);
        }
        System.Threading.Tasks.Task IProjectService.RemoveMember(int projectId, int memberId, int currentUserId)
        {
            throw new NotImplementedException();
        }
        public async System.Threading.Tasks.Task<ProjectDto> UpdateProject(int projectId, UpdateProjectDto dto, int userId, string role)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new Exception($"Проект с ID {projectId} не найден");

            if (role != "Admin" && project.ManagerId != userId)
                throw new Exception("Вы не можете редактировать этот проект");

            if (!string.IsNullOrWhiteSpace(dto.Title))
                project.Title = dto.Title;
            if (dto.Description != null)
                project.Description = dto.Description;
            if (dto.Deadline.HasValue)
            {
                if (dto.Deadline.Value <= DateTime.UtcNow)
                    throw new Exception("Дедлайн должен быть в будущем");
                project.Deadline = dto.Deadline.Value;
            }
            if (dto.Status.HasValue)
                project.Status = dto.Status.Value;

            if (dto.ParticipantIds != null)
            {
                var currentIds = project.ProjectUsers.Select(pu => pu.UserId).ToList();
                var newIds = dto.ParticipantIds.Distinct().Where(id => id != project.ManagerId).ToList();

                var toAdd = newIds.Except(currentIds).ToList();
                if (toAdd.Any())
                {
                    var usersToAdd = await _context.Users.Where(u => toAdd.Contains(u.Id)).ToListAsync();
                    foreach (var user in usersToAdd)
                    {
                        project.ProjectUsers.Add(new ProjectUser
                        {
                            ProjectId = project.Id,
                            UserId = user.Id,
                            JoinedAt = DateTime.UtcNow
                        });
                    }
                }

                var toRemove = currentIds.Except(newIds).ToList();
                if (toRemove.Any())
                {
                    bool hasActiveTasks = await _context.Tasks
                        .AnyAsync(t => t.ProjectId == projectId && toRemove.Contains(t.AssignedToId ?? 0)
                                    && t.Status != Models.Enums.TaskStatus.Completed && t.Status != Models.Enums.TaskStatus.Cancelled);
                    if (hasActiveTasks)
                        throw new Exception("Нельзя удалить участников с активными задачами");

                    var toRemoveEntities = project.ProjectUsers.Where(pu => toRemove.Contains(pu.UserId)).ToList();
                    foreach (var pu in toRemoveEntities)
                        _context.ProjectUsers.Remove(pu);
                }
            }

            await _context.SaveChangesAsync();

            return await GetProjectById(projectId, userId, role);
        }
        private async Task<string> GetUserRole(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
            return user?.UserRoles.FirstOrDefault()?.Role?.Name ?? "Member";
        }
    }
}

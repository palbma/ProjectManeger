using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Dto.Task;
using ProjectManager.Models;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Services.Implementations
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TaskService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<TaskDto>> GetAllTasks(int? projectId, int? assignedToId, ProjectManager.Models.Enums.TaskStatus? status, int userId, string role)
        {
            var query = _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .AsQueryable();

            if (projectId.HasValue)
                query = query.Where(t => t.ProjectId == projectId.Value);

            if (assignedToId.HasValue)
                query = query.Where(t => t.AssignedToId == assignedToId.Value);

            if (role != "Admin")
            {
                query = query.Where(t =>
                    t.Project.ManagerId == userId ||
                    t.AssignedToId == userId ||
                    _context.ProjectUsers.Any(pu => pu.ProjectId == t.ProjectId && pu.UserId == userId)
                );
            }

            var tasks = await query.ToListAsync();

            return tasks.Select(t => _mapper.Map<TaskDto>(t)).ToList();
        }

        public async Task<TaskDto> GetTaskById(int taskId, int userId, string role)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                throw new KeyNotFoundException($"Task with ID {taskId} was not found");

            if (role != "Admin" &&
                task.Project.ManagerId != userId &&
                task.AssignedToId != userId &&
                !_context.ProjectUsers.Any(pu => pu.ProjectId == task.ProjectId && pu.UserId == userId))
            {
                throw new UnauthorizedAccessException("You do not have access to this task");
            }

            return _mapper.Map<TaskDto>(task);
        }

        public async Task<TaskDto> CreateTask(CreateTaskDto createTaskDto, int userId)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == createTaskDto.ProjectId);

            if (project == null)
                throw new KeyNotFoundException("Project was not found");

            var task = _mapper.Map<Models.Task>(createTaskDto);

            task.CreatedById = userId;
            task.CreatedAt = DateTime.UtcNow;
            task.Status = ProjectManager.Models.Enums.TaskStatus.ToDo;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return await GetTaskById(task.Id, userId, "Admin");
        }

        public async System.Threading.Tasks.Task UpdateTask(int taskId, UpdateTaskDto updateTaskDto, int userId, string role)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                throw new KeyNotFoundException("Task was not found");

            if (role != "Admin" &&
                task.Project.ManagerId != userId &&
                task.CreatedById != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to edit this task");
            }

            if (updateTaskDto.Title != null)
                task.Title = updateTaskDto.Title;

            if (updateTaskDto.Description != null)
                task.Description = updateTaskDto.Description;

            if (updateTaskDto.Status.HasValue)
                task.Status = updateTaskDto.Status.Value;

            if (updateTaskDto.Priority.HasValue)
                task.Priority = updateTaskDto.Priority.Value;

            if (updateTaskDto.DueDate.HasValue)
                task.DueDate = updateTaskDto.DueDate.Value;

            if (updateTaskDto.AssignedToId.HasValue)
                task.AssignedToId = updateTaskDto.AssignedToId.Value;

            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task DeleteTask(int taskId, int userId, string role)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                throw new KeyNotFoundException("Task was not found");

            if (role != "Admin" &&
                task.Project.ManagerId != userId &&
                task.CreatedById != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this task");
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task AssignTask(int taskId, int assignedToId, int userId)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                throw new KeyNotFoundException("Task was not found");

            var user = await _context.Users.FindAsync(userId);
            var project = task.Project;

            if (project.ManagerId != userId)
                throw new UnauthorizedAccessException("You do not have permission to assign this task");

            task.AssignedToId = assignedToId;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task CompleteTask(int taskId, int userId)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                throw new KeyNotFoundException("Task was not found");

            if (task.AssignedToId != userId && task.CreatedById != userId)
                throw new UnauthorizedAccessException("You do not have permission to complete this task");

            task.Status = ProjectManager.Models.Enums.TaskStatus.Completed;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task ChangeTaskStatus(int taskId, ProjectManager.Models.Enums.TaskStatus newStatus, int userId, string role)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                throw new KeyNotFoundException($"Task with ID {taskId} was not found");

            bool canChangeStatus =
                role == "Admin" ||
                task.Project.ManagerId == userId ||
                task.CreatedById == userId ||
                task.AssignedToId == userId;

            if (!canChangeStatus)
                throw new UnauthorizedAccessException("You do not have permission to change the status of this task");

            task.Status = newStatus;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
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
        public async Task<TaskDto> GetTaskById(int taskId, int userId, string role)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                throw new NotImplementedException($"Завдання з ID {taskId} не знайдено");

            if (role != "Admin" && task.Project.ManagerId != userId &&
                task.AssignedToId != userId &&
                !_context.ProjectUsers.Any(pu => pu.ProjectId == task.ProjectId && pu.UserId == userId))
            {
                throw new NotImplementedException("У вас немає доступу до цього завдання");
            }

            return _mapper.Map<TaskDto>(task);
        }

        public async Task<TaskDto> CreateTask(CreateTaskDto createTaskDto, int userId) {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == createTaskDto.ProjectId);

            

            if (project == null)
                throw new DllNotFoundException("Проект не знайдено");

            var task = _mapper.Map<Models.Task>(createTaskDto);
            task.CreatedById = userId;
            task.CreatedAt = DateTime.UtcNow;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return await GetTaskById(task.Id, userId, "Admin");

        }

        public Task<List<TaskDto>> GetAllTasks(int? projectId, int? assignedToId, TaskStatus? status, int userId, string role)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task UpdateTask(int taskId, UpdateTaskDto updateTaskDto, int userId, string role)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task DeleteTask(int taskId, int userId, string role)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task AssignTask(int taskId, int assignedToId, int userId)
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task CompleteTask(int taskId, int userId)
        {
            throw new NotImplementedException();
        }
    }
}

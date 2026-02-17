using ProjectManager.Dto.Task;

namespace ProjectManager.Services.Interfaces
{
    public interface ITaskService
    {
        public interface ITaskService
        {
            Task<List<TaskDto>> GetAllTasks(int? projectId, int? assignedToId, TaskStatus? status, int userId, string role);
            Task<TaskDto> GetTaskById(int taskId, int userId, string role);
            Task<TaskDto> CreateTask(CreateTaskDto createTaskDto, int userId);
            Task UpdateTask(int taskId, UpdateTaskDto updateTaskDto, int userId, string role);
            Task DeleteTask(int taskId, int userId, string role);
            Task AssignTask(int taskId, int assignedToId, int userId);
            Task CompleteTask(int taskId, int userId);
        }
    }
}

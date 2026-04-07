using ProjectManager.Dto.Task;

namespace ProjectManager.Services.Interfaces
{
    public interface ITaskService
    {
       
            Task<List<TaskDto>> GetAllTasks(int? projectId, int? assignedToId, ProjectManager.Models.Enums.TaskStatus? status, int userId, string role);
            Task<TaskDto> GetTaskById(int taskId, int userId, string role);
            Task<TaskDto> CreateTask(CreateTaskDto createTaskDto, int userId);
            Task UpdateTask(int taskId, UpdateTaskDto updateTaskDto, int userId, string role);
            Task DeleteTask(int taskId, int userId, string role);
            Task AssignTask(int taskId, int assignedToId, int userId);
            Task CompleteTask(int taskId, int userId);
            Task ChangeTaskStatus(int taskId, ProjectManager.Models.Enums.TaskStatus newStatus, int userId, string role);
    }
}

using ProjectManager.Dto.Users;
using ProjectManager.Models.Enums;

namespace ProjectManager.Dto.Task
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public UserDto? AssignedTo { get; set; }
        public UserDto CreatedBy { get; set; }
        public int CommentCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

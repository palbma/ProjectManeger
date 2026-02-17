using ProjectManager.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Dto.Task
{
    public class UpdateTaskDto
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        public Models.Enums.TaskStatus? Status { get; set; }

        public TaskPriority? Priority { get; set; }

        public DateTime? DueDate { get; set; }

        public int? AssignedToId { get; set; }
    }
}

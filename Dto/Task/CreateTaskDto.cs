using ProjectManager.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Dto.Task
{
    public class CreateTaskDto
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public DateTime? DueDate { get; set; }

        public int? AssignedToId { get; set; }
    }
}

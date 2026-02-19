using ProjectManager.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Dto.Project
{
    public class UpdateProjectDto
    {
        [MaxLength(200)]
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public ProjectStatus? Status { get; set; }
        public List<int>? ParticipantIds { get; set; } // Оновлений список учасників
    }
}

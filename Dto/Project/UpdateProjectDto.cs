using ProjectManager.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Dto.Project
{
    public class UpdateProjectDto
    {
        [MaxLength(200, ErrorMessage = "Максимальная длина названия – 200 символов")]
        public string? Title { get; set; }
        [MaxLength(1000, ErrorMessage = "Описание не может превышать 1000 символов")]
        public string? Description { get; set; }
        [Required]
        public DateTime? Deadline { get; set; }
        [EnumDataType(typeof(ProjectStatus), ErrorMessage = "Некорректный статус проекта")]
        public ProjectStatus? Status { get; set; }
        [Required]
        public List<int>? ParticipantIds { get; set; } // Оновлений список учасників
    }
}

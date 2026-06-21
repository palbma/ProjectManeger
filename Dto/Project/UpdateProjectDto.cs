using ProjectManager.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Dto.Project
{
    public class UpdateProjectDto
    {
        [MaxLength(200, ErrorMessage = "Максимальна довжина назви — 200 символів")]
        public string? Title { get; set; }

        [MaxLength(1000, ErrorMessage = "Опис не може перевищувати 1000 символів")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Дедлайн є обов’язковим")]
        public DateTime? Deadline { get; set; }

        [EnumDataType(typeof(ProjectStatus), ErrorMessage = "Некоректний статус проєкту")]
        public ProjectStatus? Status { get; set; }

        [Required(ErrorMessage = "Список учасників є обов’язковим")]
        public List<int>? ParticipantIds { get; set; } 
    }
}
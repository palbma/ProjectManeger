using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Dto.Project
{
    public class CreateProjectDto
    {
        [Required(ErrorMessage = "Название проекта обязательно")]
        [MaxLength(200, ErrorMessage = "Максимальная длина названия – 200 символов")]
        public string Title { get; set; }
        [MaxLength(1000, ErrorMessage = "Описание не может превышать 1000 символов")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Дедлайн обязателен")]
        public DateTime Deadline { get; set; }
        [MinLength(1, ErrorMessage = "Добавьте хотя бы одного участника (кроме менеджера)")] 
        public List<int> MemberIds { get; set; } = new();
    }
}

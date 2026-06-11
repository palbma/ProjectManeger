using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Dto.Project
{
    public class CreateProjectDto
    {
        [Required(ErrorMessage = "Назва проєкту є обов’язковою")]
        [MaxLength(200, ErrorMessage = "Максимальна довжина назви — 200 символів")]
        public string Title { get; set; }

        [MaxLength(1000, ErrorMessage = "Опис не може перевищувати 1000 символів")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Дедлайн є обов’язковим")]
        public DateTime Deadline { get; set; }

        [MinLength(1, ErrorMessage = "Додайте хоча б одного учасника (окрім менеджера)")]
        public List<int> MemberIds { get; set; } = new();
    }
}
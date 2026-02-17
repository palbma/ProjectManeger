using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Dto.Project
{
    public class CreateProjectDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public DateTime Deadline { get; set; }
        public List<int> MemberIds { get; set; } = new();
    }
}

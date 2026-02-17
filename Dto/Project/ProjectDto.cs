using ProjectManager.Dto.Users;
using ProjectManager.Models.Enums;

namespace ProjectManager.Dto.Project
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public ProjectStatus Status { get; set; }
        public UserDto Manager { get; set; }
        public List<UserDto> Members { get; set; } = new();
        public int TaskCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

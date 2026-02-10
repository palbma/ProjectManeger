using ProjectManager.Models.Enums;

namespace ProjectManager.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime Deadline { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int ManagerId { get; set; }

        public User Manager { get; set; }
        public ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();
        public ICollection<Task> Tasks { get; set; } = new List<Task>();
    }
}

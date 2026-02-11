namespace ProjectManager.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ProjectId { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.ToDo;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public int? AssignedToId { get; set; }
        public int CreatedById { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Навігаційні властивості
        public Project Project { get; set; }
        public User? AssignedTo { get; set; }
        public User CreatedBy { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}

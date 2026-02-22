namespace ProjectManager.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int TaskId { get; set; }          
        public int UserId { get; set; }          
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Task Task { get; set; }
        public User User { get; set; }
    }
}

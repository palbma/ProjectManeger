namespace ProjectManager.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } =  string.Empty;      
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set;}

        public Task Task { get; set; }
        public User User { get; set; }

    }
}

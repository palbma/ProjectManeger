using ProjectManager.Dto.Users;

namespace ProjectManager.Dto.Comment
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int TaskId { get; set; }
        public UserDto User { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

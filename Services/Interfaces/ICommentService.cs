using ProjectManager.Dto.Comment;

namespace ProjectManager.Services.Interfaces
{
    public interface ICommentService
    {
        Task<List<CommentDto>> GetTaskComments(int taskId, int userId, string role);
        Task<CommentDto> CreateComment(CreateCommentDto dto, int userId);
        Task DeleteComment(int commentId, int userId, string role);
    }
}

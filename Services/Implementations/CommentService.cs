using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Dto.Comment;
using ProjectManager.Models;
using ProjectManager.Services.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace ProjectManager.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CommentService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<List<CommentDto>> GetTaskComments(int taskId, int userId, string role)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                    .ThenInclude(p => p.ProjectUsers)
                .FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
                throw new Exception($"Задача с ID {taskId} не найдена");

            bool hasAccess = role == "Admin"
                || task.Project.ManagerId == userId
                || task.Project.ProjectUsers.Any(pu => pu.UserId == userId);
            if (!hasAccess)
                throw new Exception("У вас нет доступа к комментариям этой задачи");

            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.TaskId == taskId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<CommentDto>>(comments);
        }

        public async Task<CommentDto> CreateComment(CreateCommentDto dto, int userId)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                    .ThenInclude(p => p.ProjectUsers)
                .FirstOrDefaultAsync(t => t.Id == dto.TaskId);
            if (task == null)
                throw new Exception($"Задача с ID {dto.TaskId} не найдена");

            var comment = new Comment
            {
                Content = dto.Content,
                TaskId = dto.TaskId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            var savedComment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            return _mapper.Map<CommentDto>(savedComment);
        }

        public async Task DeleteComment(int commentId, int userId, string role)
        {
            var comment = await _context.Comments
                .Include(c => c.Task)
                    .ThenInclude(t => t.Project)
                .FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
                throw new Exception($"Комментарий с ID {commentId} не найден");

            bool canDelete = role == "Admin"
                || comment.UserId == userId
                || comment.Task.Project.ManagerId == userId;

            if (!canDelete)
                throw new Exception("Вы не можете удалить этот комментарий");

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }
   
    }
}


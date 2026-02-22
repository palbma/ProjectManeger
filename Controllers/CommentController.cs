using Microsoft.AspNetCore.Mvc;
using ProjectManager.Dto.Comment;
using ProjectManager.Services.Interfaces;
using System.Security.Claims;

namespace ProjectManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }
        [HttpGet("task/{taskId}")]
        public async Task<ActionResult<List<CommentDto>>> GetTaskComments(int taskId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var role = User.FindFirstValue(ClaimTypes.Role) ?? "Member";
                var comments = await _commentService.GetTaskComments(taskId, userId, role);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CommentDto>> CreateComment(CreateCommentDto createDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var comment = await _commentService.CreateComment(createDto, userId);
                return CreatedAtAction(nameof(GetTaskComments), new { taskId = comment.TaskId }, comment);
            }

            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteComment(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var role = User.FindFirstValue(ClaimTypes.Role) ?? "Member";
                await _commentService.DeleteComment(id, userId, role);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                throw new Exception("Пользователь не авторизован");
            return int.Parse(userIdClaim);
        }
    }
}


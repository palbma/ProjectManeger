using Microsoft.AspNetCore.Mvc;
using ProjectManager.Dto.Task;
using ProjectManager.Services.Interfaces;
using System.Security.Claims;

namespace ProjectManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : Controller
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }
        [HttpPost("tasks")]
        public async Task<ActionResult<TaskDto>> CreateTask(CreateTaskDto createTaskDto)
        {

            try
            {
                var userId = GetCurrentUserId();
                var task = await _taskService.CreateTask(createTaskDto, userId);
                return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
            }
            catch (NotImplementedException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTask(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                var task = await _taskService.GetTaskById(id, userId, role);
                return Ok(task);
            }
            catch (NotImplementedException ex)
            {
                return NotFound(ex.Message);
            }
            
        }

        private int GetCurrentUserId()
        {
            // временно для теста жестко мощно и быстро задать
            var idStr = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr))
                return 1; // тестовый пользователь
            return int.Parse(idStr);
        }

    }
}

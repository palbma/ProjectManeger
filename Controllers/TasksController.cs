using Microsoft.AspNetCore.Mvc;
using ProjectManager.Dto.Task;
using ProjectManager.Models;
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
        [HttpPost]
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";
                await _taskService.DeleteTask(id, userId, role);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/assign")]
        public async Task<IActionResult> AssignTask(int id, [FromQuery] int assignedToId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _taskService.AssignTask(id, assignedToId, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteTask(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _taskService.CompleteTask(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetAllTasks(
        [FromQuery] int? projectId,
        [FromQuery] int? assignedToId,
        [FromQuery] ProjectManager.Models.Enums.TaskStatus? status)
        {
            try
            {
                var userId = GetCurrentUserId();
                var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

                var tasks = await _taskService.GetAllTasks(projectId, assignedToId, status, userId, role);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    
        private int GetCurrentUserId()
        {
            var idStr = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr))
                return 1; 
            return int.Parse(idStr);
        }
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeTaskStatus(int id, [FromBody] ChangeTaskStatusDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";
                await _taskService.ChangeTaskStatus(id, dto.Status, userId, role);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

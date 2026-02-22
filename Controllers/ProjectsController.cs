using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.Dto.Project;
using ProjectManager.Services.Interfaces;
using System.Security.Claims;

namespace ProjectManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }
        [HttpPost]

        public async Task<ActionResult<ProjectDto>> CreateProject(CreateProjectDto createProjectDto)
        {
            try
            {
                var managerId = GetCurrentUserId();
                var project = await _projectService.CreateProject(createProjectDto, managerId);
                return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
            }
            catch (NotImplementedException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }

        }
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                throw new NotImplementedException("Пользователь не авторизован");
            return int.Parse(userIdClaim);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProject(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var role = User.FindFirstValue(ClaimTypes.Role) ?? "Member";

                var project = await _projectService.GetProjectById(id, userId, role);
                return Ok(project);
            }
            catch (NotImplementedException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<ProjectDto>>> GetAllProjects()
        {
            try
            {
                var userId = GetCurrentUserId();
                var role = User.FindFirstValue(ClaimTypes.Role) ?? "Member";
                var projects = await _projectService.GetAllProjects(userId, role);
                return Ok(projects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка: {ex.Message}");
            }
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<ActionResult<ProjectDto>> UpdateProject(int id, UpdateProjectDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var role = User.FindFirstValue(ClaimTypes.Role) ?? "Member";
                var updatedProject = await _projectService.UpdateProject(id, dto, userId, role);
                return Ok(updatedProject); 
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<ActionResult> DeleteProject(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var role = User.FindFirstValue(ClaimTypes.Role) ?? "Member";
                await _projectService.DeleteProject(id, userId, role);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
            
        }
    }
}


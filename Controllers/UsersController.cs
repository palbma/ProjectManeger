using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ProjectManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        [HttpGet("test")]
        public IActionResult TestConnection()
        {
            return Ok(new
            {
                Message = "AuthController работает!",
                Timestamp = DateTime.UtcNow,
                Status = "Active"
            });
        }
    }

}


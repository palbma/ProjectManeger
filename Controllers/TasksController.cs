using Microsoft.AspNetCore.Mvc;

namespace ProjectManager.Controllers
{
    public class TasksController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

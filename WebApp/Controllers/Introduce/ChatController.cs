using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Introduce
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

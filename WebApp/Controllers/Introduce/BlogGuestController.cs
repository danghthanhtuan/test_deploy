using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Introduce
{
    public class BlogGuestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

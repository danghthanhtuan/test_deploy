using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Introduce
{
    public class AboutGuestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

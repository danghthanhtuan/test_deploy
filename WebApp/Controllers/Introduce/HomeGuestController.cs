using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Introduce
{
    public class HomeGuestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

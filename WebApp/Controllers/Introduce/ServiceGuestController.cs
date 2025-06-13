using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Introduce
{
    public class ServiceGuestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

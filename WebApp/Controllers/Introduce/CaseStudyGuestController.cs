using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Introduce
{
    public class CaseStudyGuestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [Authorize(AuthenticationSchemes = "User")]

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


    }
}

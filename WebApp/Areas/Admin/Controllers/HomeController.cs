using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WebApp.Areas.Admin.Controllers
{

    [Area("admin")]
    [Route("admin/homeadmin")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]
    public class HomeController : Controller
    {
        [Route("")]
        [Route("index")]
        [AuthorizeToken]
        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "phanquyen");
            }
            else
            {
                return View();
            }
        }

       
        
    }
}

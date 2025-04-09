using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/phanquyen")]
    public class PhanquyenController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7028/api/admin");
        private readonly HttpClient _client;

        public PhanquyenController()
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;

        }

        [AuthorizeToken]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}


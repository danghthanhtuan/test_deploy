using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApp.Configs;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/phanquyen")]
    public class PhanquyenController : Controller
    {
        private readonly HttpClient _client;
        private readonly ApiConfigs _apiConfigs;

        public PhanquyenController(IOptions<ApiConfigs> apiConfigs)
        {
            _client = new HttpClient();
            _apiConfigs = apiConfigs.Value;


        }

        [AuthorizeToken]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}


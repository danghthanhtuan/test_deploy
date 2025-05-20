using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.DTO;

namespace WebApp.Areas.Controllers
{
    public class SeeContractController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7190/api/client");
        private readonly HttpClient _client;
        public SeeContractController()
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
        }
        public IActionResult Index()
        {
            return View();
        }

        
        
    }
}

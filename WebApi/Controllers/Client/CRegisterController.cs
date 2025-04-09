using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTO;
using WebApi.Models;
using WebApi.Service.Client;

namespace WebApi.Controllers.Client
{
    [Route("api/client/[controller]/[action]")]
    [ApiController]
    public class CRegisterController : Controller
    {
        private readonly ManagementDbContext _context;
        private readonly CRegisterService _registerService;
        public CRegisterController(ManagementDbContext context, CRegisterService registerService)
        {

            _context = context;
            _registerService = registerService;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterclientDTO model)
        {
            var result = await _registerService.RegisterAsync(model);
            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }
    }
}

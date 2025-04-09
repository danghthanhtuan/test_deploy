using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.DTO;
using WebApi.Models;
using WebApi.Service.Admin;

namespace WebApi.Controllers
{
    [Route("api/admin/[controller]/[action]")]
    [ApiController]
    public class RegisterController : Controller
    {
        private readonly ManagementDbContext _context;
        private readonly RegisterService _registerService;
        public RegisterController(ManagementDbContext context, RegisterService registerService)
        {

            _context = context;
            _registerService = registerService;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
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

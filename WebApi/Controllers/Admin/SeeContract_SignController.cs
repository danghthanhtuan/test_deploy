using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;
using WebApi.Service.Admin;

namespace WebApi.Controllers.Admin
{
    [Route("api/admin/[controller]/[action]")]
    [ApiController]
    public class SeeContract_SignController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly ManagementDbContext _context;
        private readonly SeeContract_SignService _sign;
        public SeeContract_SignController(IConfiguration config, IWebHostEnvironment env, ManagementDbContext context, SeeContract_SignService sign)
        {
            _config = config;
            _env = env;
            _context = context;
            _sign = sign;
        }

        [HttpPost]
        public async Task<IActionResult> UploadSignedFile(IFormFile signedFile, [FromForm] string email, [FromForm] string originalFileName)
        {
            if (signedFile == null || signedFile.Length == 0 || originalFileName 
                == null)
                return BadRequest(new { message = "File không hợp lệ." });

            try
            {
                var fileName = await _sign.UploadSignedContract(signedFile, email, originalFileName);
                return Ok(new { message = "Upload thành công", fileName });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UploadSignedContract] Lỗi: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi khi lưu file", error = ex.Message });
            }
        }

    }
}

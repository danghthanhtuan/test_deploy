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
        private readonly PdfService _pdfService;

        public SeeContract_SignController(IConfiguration config, IWebHostEnvironment env, ManagementDbContext context, SeeContract_SignService sign, PdfService pdfService)
        {
            _config = config;
            _env = env;
            _context = context;
            _sign = sign;
            _pdfService = pdfService;
        }

        //update db client ký xong.
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

        [HttpPost]
        public async Task<IActionResult> SignPdfWithPfx(IFormFile pfxFile, [FromForm] string password, [FromForm] string fileName, [FromForm] string email)
        {
            Console.WriteLine("---- Log đầu vào ----");
            Console.WriteLine("fileName: " + fileName);
            Console.WriteLine("email: " + email);
            Console.WriteLine("password: " + password);
            Console.WriteLine("pfxFile: " + (pfxFile != null ? pfxFile.FileName : "null"));
            if (pfxFile == null || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fileName))
            {
                return BadRequest("Thiếu thông tin bắt buộc");
            }
            try
            {
                // Xác định đường dẫn đến thư mục chứa file PDF cần ký
                var pdfFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "signed-contracts");
                var filePath = Path.Combine(pdfFolder, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Không tìm thấy file PDF gốc để ký.");
                }

                var originalPdfBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                using var pfxStream = pfxFile.OpenReadStream();

                var signedPdfBytes = _pdfService.SignPdfWithClientCertificate(originalPdfBytes, pfxStream, password, email);

                // Trả về file PDF đã ký
                return File(signedPdfBytes, "application/pdf", "signed_" + fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi ký PDF: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveSignedPdf(IFormFile signedPdf, [FromForm] string fileName, [FromForm]string email)
        {
            if (signedPdf == null || string.IsNullOrEmpty(fileName)|| string.IsNullOrEmpty(email))
                return BadRequest("Thiếu thông tin đầu vào");

            var result = await _sign.SaveSignedPdfAsync(signedPdf, fileName, email);

            if (!result.Success)
                return StatusCode(500, result.Message);

            return Ok("Lưu file và cập nhật DB thành công.");
        }

        [HttpPost]
        public async Task<IActionResult> UploadSignatureImage(IFormFile signatureImage)
        {
            if (signatureImage == null || signatureImage.Length == 0)
                return BadRequest("Chưa có ảnh chữ ký.");

            using var stream = signatureImage.OpenReadStream();
            var pdfBytes = await _pdfService.InsertSignatureToContractAsync(stream);

            return File(pdfBytes, "application/pdf");
        }

    }
}

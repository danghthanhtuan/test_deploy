using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;
using WebApi.Service.Client;

namespace WebApi.Controllers.Admin
{
    [Route("api/admin/[controller]/[action]")]
    [ApiController]
    public class SeeContract_SignController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _env;
        private readonly ManagementDbContext _context;
        public SeeContract_SignController(IConfiguration config, EmailService emailService, IWebHostEnvironment env, ManagementDbContext context )
        {
            _config = config;
            _emailService = emailService;
            _env = env;
            _context = context;
        }

        [HttpPost("sign")]
        public async Task<IActionResult> SignContract([FromForm] IFormFile signedPdf, [FromForm] string email, [FromForm] string fileName)
        {
            try
            {
                if (signedPdf == null || signedPdf.Length == 0)
                    return BadRequest(new { success = false, message = "File PDF chưa được gửi lên." });

                // 1. Tìm tài khoản theo email
                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Rootaccount == email);
                if (account == null)
                    return NotFound(new { success = false, message = "Không tìm thấy tài khoản." });

                // 2. Tìm ContractFile theo tên file
                var contractFile = await _context.ContractFiles.FirstOrDefaultAsync(cf => cf.ConfileName == fileName);
                if (contractFile == null)
                    return NotFound(new { success = false, message = "Không tìm thấy file hợp đồng." });

                // 3. Tìm hợp đồng theo CustomerId và ContractNumber
                var contract = await _context.Contracts.FirstOrDefaultAsync(c =>
                    c.Customerid == account.Customerid &&
                    c.Contractnumber == contractFile.Contractnumber);
                if (contract == null)
                    return NotFound(new { success = false, message = "Không tìm thấy hợp đồng để ký." });

                string oldStatus = contract.Constatus;

                // 4. Lưu file PDF mới vào thư mục signed-contracts
                string signedFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "signed-contracts");
                if (!Directory.Exists(signedFolder))
                    Directory.CreateDirectory(signedFolder);

                string signedFilePath = Path.Combine(signedFolder, fileName);

                if (System.IO.File.Exists(signedFilePath))
                    System.IO.File.Delete(signedFilePath);

                // Lưu file
                using (var stream = new FileStream(signedFilePath, FileMode.Create))
                {
                    await signedPdf.CopyToAsync(stream);
                }

                // 5. Cập nhật hợp đồng trạng thái
                contract.Constatus = "Đã ký";

                // 6. Thêm bản ghi mới file đã ký vào ContractFiles
                var signedContractFile = new ContractFile
                {
                    Contractnumber = contract.Contractnumber,
                    ConfileName = fileName,
                    FilePath = signedFilePath,
                    UploadedAt = DateTime.Now,
                    FileStatus = "Đã ký"
                };
                _context.ContractFiles.Add(signedContractFile);

                // 7. Thêm vào bảng ContractStatusHistory
                var statusHistory = new ContractStatusHistory
                {
                    Contractnumber = contract.Contractnumber,
                    OldStatus = oldStatus,
                    NewStatus = "Đã ký",
                    ChangedAt = DateTime.Now,
                    ChangedBy = email
                };
                _context.ContractStatusHistories.Add(statusHistory);

                // 8. Lưu thay đổi
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Đã ký hợp đồng thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi ký hợp đồng: " + ex.Message });
            }
        }

    }
}

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTO;
using WebApi.Models;
using WebApi.Service.Admin;
using WebApi.Service.Client;
using Microsoft.Extensions.Caching.Memory;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using Microsoft.AspNetCore.Http.HttpResults;
namespace WebApi.Controllers.Admin
{
    [Route("api/admin/[controller]/[action]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;
        private readonly ManagementDbContext _context;
        private readonly IMapper _mapper;
        private readonly PdfService _pdfService;
        private readonly IWebHostEnvironment _env;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;
        
        public AccountController(IMapper mapper, ManagementDbContext context, AccountService accountService, PdfService pdfService, IWebHostEnvironment env, EmailService emailService, IConfiguration config)
        {
            _context = context;
            _mapper = mapper;
            _accountService = accountService;
            _pdfService = pdfService;
            _env = env;
            _emailService = emailService;
            _config = config;
          
        }

        [Authorize(Roles = "Admin,HanhChinh,Director")]
        [HttpPost]
        public async Task<ActionResult<CompanyAccountDTO>> GetAllCompany([FromBody] GetListCompanyPaging req)
        {
            var company = await _accountService.GetAllCompany(req);
            return Ok(company);
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpPut]
        public IActionResult UpdateStatus([FromBody] updateID updateID)
        {
            try
            {
                Console.WriteLine($"Received UpdateStatus request: CustomerId = {updateID.CustomerId}, Status = {updateID.status}");

                if (_accountService.UpdateStatus(updateID.status, updateID.CustomerId))
                {
                    return Ok(new APIResponse<object>()
                    {
                        Success = true,
                        Message = "Cập nhật thành công",
                        Data = null
                    });
                }
                else
                {
                    return Ok(new APIResponse<object>()
                    {
                        Success = false,
                        Message = "Cập nhật không thành công! Đã xảy ra lỗi",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật: " + ex.Message);
                return BadRequest(ex.Message);
            }
        }       

        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public IActionResult Update([FromBody] CompanyAccountDTO companyAccountDTO, [FromQuery] string id)
        {
            if (companyAccountDTO == null || string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var company = _accountService.Update(companyAccountDTO, id);
            if (company.StartsWith("IT030300"))
            {
                return Ok(new
                {
                    success = true,
                    message = "Cập nhật tài khoản thành công",
                    companyID = company
                });
            }
            return BadRequest(new { success = false, message = company });
        }

        [HttpPost]
        public async Task<IActionResult> ExportToCsv([FromBody] ExportRequestDTO request)
        {
            try
            {
                byte[] fileBytes = await _accountService.ExportToCsv(request);
                return File(fileBytes, "text/csv", "DanhSachKhachHang.csv");
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpGet]
        public async Task<ActionResult<ServiceTypeDTO2>> GetListServiceID()
        {
            var regu = await _accountService.GetListServiceID();
            return Ok(regu);
        }

        //TẠO FILE CHỜ SẾP KÝ
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public async Task<IActionResult> GenerateContract([FromBody] CompanyAccountDTO dto, [FromQuery] string id)
        {
            try
            {
                string contractId = Guid.NewGuid().ToString();

                // 1. Tạo file PDF gốc chưa ký
                byte[] pdfBytes = _pdfService.GenerateContractPdf(dto);

                // 3. Lưu file PDF chờ boss ký
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp-pdfs");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = $"{contractId}.pdf";
                string fullPath = Path.Combine(folderPath, fileName);
                //await System.IO.File.WriteAllBytesAsync(fullPath, signedPdfBytes);
                await System.IO.File.WriteAllBytesAsync(fullPath, pdfBytes);

                // 5. Lưu thông tin hợp đồng, trạng thái file đã ký vào DB
                var result = await _accountService.SaveContractStatusAsync(new CompanyContractDTOs
                {
                    CustomerId = dto.CustomerId,
                    CompanyName = dto.CompanyName,
                    TaxCode = dto.TaxCode,
                    CompanyAccount = dto.CompanyAccount,
                    AccountIssuedDate = dto.AccountIssuedDate,
                    CPhoneNumber = dto.CPhoneNumber,
                    CAddress = dto.CAddress,
                    RootAccount = dto.RootAccount,
                    RootName = dto.RootName,
                    RPhoneNumber = dto.RPhoneNumber,
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    ContractNumber = dto.ContractNumber,
                    Startdate = dto.Startdate,
                    Enddate = dto.Enddate,
                    CustomerType = dto.CustomerType,
                    ServiceType = dto.ServiceType,
                    ConfileName = fileName,
                    FilePath = fullPath,
                    ChangedBy = id,
                    Amount = dto.Amount,
                    Original = dto.Original,
                });

                if (result == null || result.StartsWith("Lỗi") || result.Contains("không tồn tại") || result.Contains("đã tồn tại"))
                {
                    return BadRequest(new { success = false, message = result });
                }

                //// 6. Gửi mail hợp đồng tới khách hàng ký
                //await _emailService.SendContractEmail(dto.RootAccount, dto.CompanyName, viewLink);

                return Ok(new { success = true, fullPath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi gửi email hoặc tạo hợp đồng: {ex.Message}");
            }
        }

        //lấy list hiển thị
        [Authorize(Roles = "Admin,Director")]
        [HttpPost]
        public async Task<ActionResult<CompanyContractDTOs>> GetListPending([FromBody] GetListCompanyPaging req)
        {
            var regu = await _accountService.GetListPending(req);
            return Ok(regu);
        }

        //boss kí
        [Authorize(Policy = "DirectorPolicy")]
        [HttpPost]
        public async Task<IActionResult> SignPdfWithAdminCertificate([FromBody] SignAdminRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.FilePath) || string.IsNullOrEmpty(request.StaffId) || string.IsNullOrEmpty(request.ContractNumber))
                    return BadRequest(new { success = false, message = "Thiếu thông tin file, mã hợp đồng hoặc nhân viên." });

                if (!System.IO.File.Exists(request.FilePath))
                    return NotFound(new { success = false, message = "File hợp đồng không tồn tại." });

                // Đọc file gốc
                byte[] originalPdfBytes = await System.IO.File.ReadAllBytesAsync(request.FilePath);

                // Ký file
                byte[] signedPdfBytes = _pdfService.SignPdfWithAdminCertificate(originalPdfBytes, request.StaffId);

                // 3. Tạo đường dẫn lưu file mới
                var folderPath = Path.Combine(
                    _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                    "signed-contracts"
                );
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // 4. Tạo tên file mới theo timestamp
                string newFileName = $"{Path.GetFileNameWithoutExtension(request.FilePath)}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string newFilePath = Path.Combine(folderPath, newFileName);

                // 5. Ghi file đã ký vào thư mục mới
                await System.IO.File.WriteAllBytesAsync(newFilePath, signedPdfBytes);

                try
                {
                    if (System.IO.File.Exists(request.FilePath))
                        System.IO.File.Delete(request.FilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Không thể xóa file gốc: {ex.Message}");
                }


                // Gọi hàm cập nhật DB (không lưu file nữa, chỉ cập nhật trạng thái, lịch sử, ContractFile)
                request.FilePath = newFilePath; // Gán lại đường dẫn mới để upload
                string result = await _accountService.UploadDirectorSigned(request);

                // Trả về phản hồi thành công nếu update cũng thành công
                if (result.Contains("thành công") || result.EndsWith(".pdf"))
                {
                    string relativePath = request.FilePath.Replace(Directory.GetCurrentDirectory(), "").Replace("\\", "/");
                    return Ok(new
                    {
                        success = true,
                        message = "Đã ký thành công và cập nhật dữ liệu.",
                        signedFilePath = relativePath
                    });
                }

                // Nếu UploadSignedContract trả về lỗi
                return StatusCode(500, new { success = false, message = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi ký hợp đồng: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống khi ký file." });
            }
        }

        //Gửi client
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public async Task<IActionResult> SendEmailtoclient([FromBody] SignAdminRequest dto)
        {
            try
            {
                // Gửi mail hợp đồng tới khách hàng ký
                await _emailService.SendEmailtoclient(dto);
                await _accountService.UploadSendclient(dto);
                return Ok(new { success = true, dto.FilePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi gửi email hoặc tạo hợp đồng: {ex.Message}");
            }
        }

        //admin duyệt
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public async Task<IActionResult> Insert([FromBody] SignAdminRequest request)
        {
            if (request == null )
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var result = await _accountService.Insert(request);

            if (result?.StartsWith("IT030300") == true)  // Kiểm tra null trước
            {
                return Ok(new
                {
                    success = true,
                    message = "Đăng ký tài khoản thành công",
                    companyID = result
                });
            }
            else
            {
                return BadRequest(new { success = false, message = result ?? "Lỗi không xác định." });
            }

        }
    }
}

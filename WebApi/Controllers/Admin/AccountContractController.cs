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
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.EntityFrameworkCore;
namespace WebApi.Controllers.Admin
{
    [Route("api/admin/[controller]/[action]")]
    [ApiController]
    public class AccountContractController : Controller
    {
        private readonly AccountContractService _accountContractService;
        private readonly ManagementDbContext _context;
        private readonly IMapper _mapper;
        private readonly PdfService _pdfService;
        private readonly IWebHostEnvironment _env;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;
        //private readonly IMemoryCache _contractCache;

        //public AccountContractController(IMapper mapper, ManagementDbContext context, AccountContractService accountContractService)
        //{
        //    _context = context;
        //    _mapper = mapper;
        //    _accountContractService = accountContractService;

        //}
        public AccountContractController(IMapper mapper, ManagementDbContext context, AccountContractService accountContractService, PdfService pdfService, IWebHostEnvironment env, EmailService emailService, IConfiguration config)
        {
            _context = context;
            _mapper = mapper;
            _accountContractService = accountContractService;
            _pdfService = pdfService;
            _env = env;
            _emailService = emailService;
            _config = config;
          
        }

        [Authorize(Roles = "Admin,HanhChinh")]
        [HttpPost]
        public async Task<ActionResult<CompanyAccountDTO>> GetAllCompany([FromBody] GetListCompanyPaging req)
        {
            var company = await _accountContractService.GetAllCompany(req);
            return Ok(company);
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpPut]
        public IActionResult UpdateStatus([FromBody] updateID updateID)
        {
            try
            {
                Console.WriteLine($"Received UpdateStatus request: CustomerId = {updateID.CustomerId}, Status = {updateID.status}");

                if (_accountContractService.UpdateStatus(updateID.status, updateID.CustomerId))
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
        public async Task<IActionResult> Insert([FromBody] CompanyAccountDTO companyAccountDTO, [FromQuery] string id)
        {
            if (companyAccountDTO == null || string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var result = await _accountContractService.Insert(companyAccountDTO, id);

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

        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public IActionResult Update([FromBody] CompanyAccountDTO companyAccountDTO, [FromQuery] string id)
        {
            if (companyAccountDTO == null || string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var company = _accountContractService.Update(companyAccountDTO, id);
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
                byte[] fileBytes = await _accountContractService.ExportToCsv(request);
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
            var regu = await _accountContractService.GetListServiceID();
            return Ok(regu);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateContractLink([FromBody] CompanyAccountDTO dto, [FromQuery] string id)
        {
            try
            {
                string contractId = Guid.NewGuid().ToString();

                // 1. Tạo file PDF gốc chưa ký
                byte[] pdfBytes = _pdfService.GenerateContractPdf(dto);

                // 2. Gọi hàm ký số file PDF (trả về file PDF đã ký)
                byte[] signedPdfBytes = _pdfService.SignPdfWithAdminCertificate(pdfBytes, id);

                // 3. Lưu file PDF đã ký
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp-pdfs");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = $"{contractId}.pdf";
                string fullPath = Path.Combine(folderPath, fileName);
                await System.IO.File.WriteAllBytesAsync(fullPath, signedPdfBytes);

                // 4. Tạo link xem file PDF
                string razorDomain = _config["App:RazorBaseUrl"] ?? "https://localhost:7176";
                string viewLink = $"{razorDomain}/SeeContract/Index?fileName={fileName}&email={dto.RootAccount}";

                // 5. Lưu thông tin hợp đồng, trạng thái file đã ký vào DB
                var result = await _accountContractService.SaveContractStatusAsync(new CompanyContractDTOs
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

                // 6. Gửi mail hợp đồng đã ký số tới khách hàng
                await _emailService.SendContractEmail(dto.RootAccount, dto.CompanyName, viewLink);

                return Ok(new { success = true, viewLink });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi gửi email hoặc tạo hợp đồng: {ex.Message}");
            }
        }

    }
}

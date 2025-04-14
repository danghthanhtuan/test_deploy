using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTO;
using WebApi.Models;
using WebApi.Service.Admin;
using WebApi.Service.Client;

namespace WebApi.Controllers
{
    [Route("api/admin/[controller]/[action]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;
        private readonly ManagementDbContext _context;
        private readonly IMapper _mapper;
        public AccountController(IMapper mapper, ManagementDbContext context, AccountService accountService)
        {
            _context = context;
            _mapper = mapper;
            _accountService = accountService;
        }

        [Authorize(Policy = "AdminPolicy")]
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
        public async Task<IActionResult> Insert([FromBody] CompanyAccountDTO companyAccountDTO, [FromQuery] string id)
        {
            if (companyAccountDTO == null || string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var result = await _accountService.Insert(companyAccountDTO, id);

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

        //[HttpGet("reset-password")]
        //public async Task<IActionResult> ResetPassword(string token)
        //{
        //    var db = _redis.GetDatabase();
        //    var email = await db.StringGetAsync($"reset_password:{token}");

        //    if (string.IsNullOrEmpty(email))
        //    {
        //        return BadRequest("Token không hợp lệ hoặc đã hết hạn.");
        //    }

        //    return Ok(new { email = email.ToString(), token });
        //}

        //[HttpPost]
        //public async Task<IActionResult> UpdatePassword([FromBody] LoginRequesta model)
        //{
        //    var result = await _accountService.UpdatePassword(model);
        //    if (!result.Success)
        //    {
        //        return BadRequest(new { success = false, message = result.Message });
        //    }

        //    return Ok(new { success = true, message = result.Message });
        //}
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTO;
using WebApi.Models;
using WebApi.Service.Admin;

namespace WebApi.Controllers.Admin
{
    [Route("api/admin/[controller]/[action]")]
    [ApiController]
    public class AccountContractController : Controller
    {
        private readonly AccountContractService _accountContractService;
        private readonly ManagementDbContext _context;
        private readonly IMapper _mapper;
        public AccountContractController(IMapper mapper, ManagementDbContext context, AccountContractService accountContractService)
        {
            _context = context;
            _mapper = mapper;
            _accountContractService = accountContractService;
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
    }
}

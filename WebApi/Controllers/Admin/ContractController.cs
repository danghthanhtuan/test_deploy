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
    public class ContractController : ControllerBase
    {
        private readonly ManagementDbContext _context;
        private readonly IMapper _mapper;
        private readonly ContractService _contractService;
        private readonly AccountService _accountService;
        public ContractController(IMapper mapper, ManagementDbContext context, ContractService contractService, AccountService accountService)
        {
            _mapper = mapper;
            _contractService = contractService;
            _context = context;
            _accountService = accountService;
        }
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public async Task<ActionResult<CompanyAccountDTO>> GetAllCompany([FromBody] GetListCompanyPaging req)
        {
            var company = await _contractService.GetAllCompany(req);
            return Ok(company);
        }
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public IActionResult InsertExtend([FromBody] ContractDTO contractDTO, [FromQuery] string id)
        {
            if (contractDTO == null || string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var result =  _contractService.InsertExtend(contractDTO, id);

            if (result == null)
            {
                return Ok(new
                {
                    success = true,
                    message = "Gia hạn tài khoản thành công",
                    companyID = result
                });
            }
            else
            {
                return BadRequest(new { success = false, message = result });
            }

        }
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public async Task<IActionResult> InsertContract([FromBody] CompanyAccountDTO companyAccountDTO, [FromQuery] string id)
        {
            if (companyAccountDTO == null || string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var result = await _contractService.InsertContract(companyAccountDTO, id);

            if (result?.StartsWith("SV0") == true)  // Kiểm tra null trước
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
        public IActionResult InsertUpgrade([FromBody] ContractDTO contractDTO, [FromQuery] string id)
        {
            if (contractDTO == null || string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var result = _contractService.InsertUpgrade(contractDTO, id);

            if (result == null)
            {
                return Ok(new
                {
                    success = true,
                    message = "Nâng cấp tài khoản thành công",
                    companyID = result
                });
            }
            else
            {
                return BadRequest(new { success = false, message = result });
            }

        }
    }
}

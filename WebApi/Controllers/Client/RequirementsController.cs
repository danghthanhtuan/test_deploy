using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTO;
using WebApi.Models;
using WebApi.Service.Admin;
using WebApi.Service.Client;

namespace WebApi.Controllers.Client
{
    [Route("api/client/[controller]/[action]")]
    [ApiController]
    public class RequirementsController : Controller
    {
        private readonly ManagementDbContext _context;
        private readonly IMapper _mapper;
        private readonly RequirementService _requirementService;
        public RequirementsController(IMapper mapper, ManagementDbContext context, RequirementService requirementService)
        {
            _mapper = mapper;
            _requirementService = requirementService;
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<CompanyAccountDTO>> GetAllInfor([FromBody] reqSelect reqSelect)
        {
            var company = await _requirementService.GetAllInfor(reqSelect.CustomerId, reqSelect.ContractNumber);
            return Ok(company);
        }


        [HttpPost]
        public IActionResult Insert([FromBody] Requirement_C Req)
        {
            if (Req == null || string.IsNullOrEmpty(Req.ContractNumber))
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var result = _requirementService.Insert(Req);

            if (result.StartsWith("RS00"))
            {
                return Ok(new
                {
                    success = true,
                    message = "Tạo yêu cầu thành công",
                    requestID = result
                });
            }
            else
            {
                return BadRequest(new { success = false, message = result });
            }
        }

        //xem chi tiet
        [HttpGet]
        public async Task<ActionResult<Requirement_Company>> GetdetailRequest([FromQuery] string query)
        {

            var sup = await _requirementService.GetdetailRequest(query);
            return Ok(sup);
        }

        //list hiển thị 
        [HttpPost]
        public async Task<ActionResult<PagingResult<Request_GroupCompany_DTO>>> GetAllRequest([FromBody] GetListReq req)
        {
            var phieuTra = await _requirementService.GetAllRequest(req);

            return Ok(phieuTra);
        }

        [HttpPost]
        public async Task<IActionResult> Review([FromBody] ReviewDTO request)
        {
            if (request == null)
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var result = await _requirementService.Review(request);

            if (result?.StartsWith("REVIEW_") == true)  // Kiểm tra null trước
            {
                return Ok(new
                {
                    success = true,
                    message = "Đánh giá dịch vụ thành công",
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

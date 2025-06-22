using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTO;
using WebApi.Models;
using WebApi.Service.Admin;
using WebApi.Service.Client;

namespace WebApi.Controllers.Admin
{
    [Route("api/admin/[controller]/[action]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly ManagementDbContext _context;
        private readonly IMapper _mapper;
        private readonly RequestService _requestService;
        public RequestController(IMapper mapper, ManagementDbContext context, RequestService requestService)
        {
            _mapper = mapper;
            _requestService = requestService;
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<Requirement_Company>> GetAllRequest([FromBody] GetListReqad aa)
        {
            var sup = await _requestService.GetAllRequest(aa);
            return Ok(sup);
        }
        [HttpGet]
        public async Task<ActionResult<CompanyAccountDTO>> GetAllInfor([FromQuery] string req)
        {
            var company = await _requestService.GetAllInfor(req);
            return Ok(company);
        }
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public IActionResult Insert([FromBody] Requirement_C Req , [FromQuery] string id)
        {
            if (Req == null || string.IsNullOrEmpty(Req.ContractNumber))
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var result = _requestService.Insert(Req,id);

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
        [HttpGet]
        public async Task<ActionResult<Requirement_Company>> GetRequestByID([FromQuery] string req)
        {
            var company = await _requestService.GetRequestByID(req);
            return Ok(company);
        }

        [HttpPut]
        public IActionResult UpdateStatus([FromBody] historyRequest historyReq)
        {
            try
            {
                Console.WriteLine($"Received UpdateStatus request: RequirementsId = {historyReq.Requirementsid}, Status = {historyReq.Apterstatus}");

                var result = _requestService.UpdateStatus(historyReq);

                if (result == null)
                {
                    return Ok(new APIResponse<object>
                    {
                        Success = true,
                        Message = "Cập nhật thành công",
                        Data = null
                    });
                }
                else
                {
                    return BadRequest(new APIResponse<object>
                    {
                        Success = false,
                        Message = result, // Trả về lỗi cụ thể từ service
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật: " + ex.Message);
                return BadRequest(new APIResponse<object>
                {
                    Success = false,
                    Message = "Lỗi hệ thống: " + ex.Message,
                    Data = null
                });
            }
        }


        [HttpGet]
        public async Task<ActionResult<HistoryRequests>> getHIS([FromQuery] string req)
        {
            var company = await _requestService.getHIS(req);
            return Ok(company);
        }
    }
}

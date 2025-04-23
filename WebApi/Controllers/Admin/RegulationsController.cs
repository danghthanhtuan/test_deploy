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
    public class RegulationsController : ControllerBase
    {
        private readonly ManagementDbContext _context;
        private readonly IMapper _mapper;
        private readonly RegulationsService _regulationsService;
        public RegulationsController(IMapper mapper, ManagementDbContext context, RegulationsService regulationsService)
        {
            _mapper = mapper;
            _context = context;
            _regulationsService = regulationsService;
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public async Task<ActionResult<RegulationsDTO>> GetAllRegulations([FromBody] GetListReq req)
        {
            var regu = await _regulationsService.GetAllRegulations(req);
            return Ok(regu);
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public async Task<ActionResult<EndowDTO>> GetAllEndow([FromBody] GetListReq req)
        {
            var regu = await _regulationsService.GetAllEndow(req);
            return Ok(regu);
        }
        
        //thêm 1 nhóm dịch vụ mới ---có nhóm mới tên 1 cái 
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public IActionResult InsertRegulation([FromBody] RegulationsDTO regu, [FromQuery] string id)
        {
            if (regu == null || string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var result = _regulationsService.InsertRegulation(regu, id);

            if (result == null)
            {
                return Ok(new
                {
                    success = true,
                    message = "Thêm nhóm dịch vụ mới thành công",
                    serviceId = result
                });
            }
            else
            {
                return BadRequest(new { success = false, message = result });
            }

        }
        
        //sửa tên nhóm dịch vụ
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public IActionResult Update([FromBody] RegulationsDTO regu, [FromQuery] string id)
        {
            if (regu == null || string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var company = _regulationsService.Update(regu, id);
            if (company.StartsWith("SER0"))
            {
                return Ok(new
                {
                    success = true,
                    message = "Cập nhật Thông tin nhóm dịch vụ thành công",
                    companyID = company
                });
            }
            return BadRequest(new { success = false, message = company });
        }
        
        //thêm 1 tên mới
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public IActionResult InsertTypename([FromBody] RegulationsDTO regu, [FromQuery] string id)
        {
            if (regu == null || string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var result = _regulationsService.InsertTypename(regu, id);

            if (result == null)
            {
                return Ok(new
                {
                    success = true,
                    message = "Thêm dịch vụ mới thành công",
                    serviceId = result
                });
            }
            else
            {
                return BadRequest(new { success = false, message = result });
            }

        }
        
        //sửa tên 1 dịch vụ 
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public IActionResult UpdateTypename([FromBody] RegulationsDTO regu, [FromQuery] string id)
        {
            if (regu == null || string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var company = _regulationsService.UpdateTypename(regu, id);
            if (company.StartsWith("SER0"))
            {
                return Ok(new
                {
                    success = true,
                    message = "Cập nhật Thông tin dịch vụ thành công",
                    companyID = company
                });
            }
            return BadRequest(new { success = false, message = company });
        }

        //xóa tên 1 dịch vụ
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public IActionResult DeleteTypename([FromBody] RegulationsDTO regu, [FromQuery] string id)
        {
            if (regu == null || string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var company = _regulationsService.DeleteTypename(regu, id);
            if (company == null)
            {
                return Ok(new
                {
                    success = true,
                    message = "Xóa dịch vụ thành công",
                    companyID = company
                });
            }
            return BadRequest(new { success = false, message = company });
        }

        //Thêm ưu đãi 
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public IActionResult InsertEndow([FromBody] EndowDTO endow, [FromQuery] string id)
        {
            if (endow == null || string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var result = _regulationsService.InsertEndow(endow, id);

            if (result == null)
            {
                return Ok(new
                {
                    success = true,
                    message = "Thêm ưu đãi mới thành công",
                    serviceId = result
                });
            }
            else
            {
                return BadRequest(new { success = false, message = result });
            }
        }

        //Sửa ưu đãi
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public IActionResult UpdateEndow([FromBody] EndowDTO endow, [FromQuery] string id)
        {
            if (endow == null || string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var company = _regulationsService.UpdateEndow(endow, id);
            if (company.StartsWith("ENDOW"))
            {
                return Ok(new
                {
                    success = true,
                    message = "Cập nhật ưu đãi thành công",
                    companyID = company
                });
            }
            return BadRequest(new { success = false, message = company });
        }

        //Lấy list dịch vụ để dropdown 
        [Authorize(Policy = "AdminPolicy")]
        [HttpGet]
        public async Task<ActionResult<ServiceGroup>> GetListServiceID()
        {
            var regu = await _regulationsService.GetListServiceID();
            return Ok(regu);
        }
    }
}

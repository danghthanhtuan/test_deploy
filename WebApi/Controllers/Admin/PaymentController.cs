using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Service.Admin;

namespace WebApi.Controllers.Admin
{
    [Route("api/admin/[controller]/[action]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _PaymentService;

        public PaymentController(PaymentService PaymentService)
        {
            _PaymentService = PaymentService;
        }

        [HttpPut]
        public IActionResult CapNhatThanhToan([FromQuery] string maHopDong, [FromQuery] string maGiaoDich, [FromQuery] string email, [FromQuery] string phuongThuc)
        {
            bool result = _PaymentService.ThanhToan(maHopDong, maGiaoDich, email, phuongThuc);
            if (result)
            {
                return Ok("Cập nhật thành công");
            }
            return BadRequest("Cập nhật thất bại");
        }


    }
}

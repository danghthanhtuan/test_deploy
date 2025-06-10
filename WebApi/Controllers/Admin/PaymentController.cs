using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTO;
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

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var payment = await _PaymentService.CreatePaymentAsync(request);
            return Ok(new { 
                payment.Id,
                payment.Amount
            }); // lấy id của giao dịch mới insert
        }

        [HttpPost]
        public IActionResult CapNhatThanhToan([FromBody] ThanhToanRequest request)
        {
            bool result = _PaymentService.ThanhToan(request.ID, request.MaGiaoDich,  request.PhuongThuc, request.TinhTrang);
            if (result)
            {
                return Ok("Cập nhật thanh toán thành công.");
            }
            return BadRequest("Cập nhật thanh toán thất bại.");
        }

        [HttpGet]
        public async Task<IActionResult> GetByContractNumber([FromQuery] string contractNumber)
        {
            if (string.IsNullOrEmpty(contractNumber))
                return BadRequest("Contract number is required");

            var data = await _PaymentService.GetByContractNumberAsync(contractNumber);

            if (data == null)
                return NotFound("Không tìm thấy hợp đồng");

            return Ok(data);
        }
    }
}

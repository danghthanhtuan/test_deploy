using Microsoft.AspNetCore.Mvc;
using WebApi.DTO;
using WebApi.Service.Admin;

namespace WebApi.Controllers.Admin
{
    [Route("api/admin/[controller]/[action]")]
    [ApiController]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRead([FromBody] int notificationId)
        {
            var result = await _notificationService.Update(notificationId);
            if (!result)
            {
                return BadRequest(new { success = false });
            }

            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> GetList([FromBody] GetListNotificationPaging req)
        {
            var result = await _notificationService.GetListPaging(req);
            if (!result.Success)
            {
                return BadRequest(new { success = false, message = "Có lỗi xảy ra." });
            }

            return Ok(new { success = true, data = result.res });
        }
    }
}

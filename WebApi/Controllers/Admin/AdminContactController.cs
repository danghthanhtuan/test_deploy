using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;
using System.Net.Http;
using WebApi.DTO;
using WebApi.Service.Admin;

namespace WebApi.Controllers.Admin
{
    [Route("api/admin/[controller]/[action]")]
    [ApiController]
    public class AdminContactController : Controller
    {
        private readonly IAdminContactService _adminContactService;
        public AdminContactController(IAdminContactService adminContactService)
        {
            _adminContactService = adminContactService;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateContactReq req)
        {
            var result = await _adminContactService.UpdateStatus(req.Id, req.Status);
            if (!result)
            {
                return BadRequest(new { success = false });
            }

            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> GetList([FromBody] GetListContactPaging req)
        {
            var result = await _adminContactService.GetListPaging(req);
            if (!result.Success)
            {
                return BadRequest(new { success = false, message = "Có lỗi xảy ra." });
            }

            return Ok(new { success = true, data = result.res });
        }

        [HttpGet]
        public async Task<IActionResult> GetContactById([FromQuery] int id)
        {
            var result = await _adminContactService.GetById(id);
            if (!result.Success)
            {
                return BadRequest();
            }

            return Ok(result.res);
        }
    }
}

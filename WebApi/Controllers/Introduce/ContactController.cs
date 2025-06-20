using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTO;
using WebApi.Service.Admin;
using WebApi.Service.Introduce;

namespace WebApi.Controllers.Introduce
{
    [Route("api/introduce/[controller]/[action]")]
    [ApiController]
    public class ContactController : Controller
    {
        private readonly ContactService _contactService;
        public ContactController(ContactService contactService)
        {
            _contactService = contactService;
        }

        //nhận thông tin liên hệ lưu db 
        [HttpPost]
        public async Task<IActionResult> CreateContact([FromBody] CreateContactDTO model)
        {
            var result = await _contactService.CreateContact(model);
            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }


        [HttpGet]
        public async Task<ActionResult<ServiceTypeDTO2>> GetListServiceID()
        {
            var regu = await _contactService.GetListServiceID();
            return Ok(regu);
        }
    }
}

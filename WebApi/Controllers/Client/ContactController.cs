using Microsoft.AspNetCore.Mvc;
using WebApi.DTO;
using WebApi.Service.Admin;
using WebApi.Service.Client;

namespace WebApi.Controllers.Client
{
    [Route("api/client/[controller]/[action]")]
    [ApiController]
    public class ContactController : Controller
    {
        private readonly ContactService _contactService;
        public ContactController(ContactService contactService)
        {
            _contactService = contactService;
        }

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
    }
}

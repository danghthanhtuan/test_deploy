using Microsoft.AspNetCore.Mvc;
using WebApi.DTO;
using WebApi.Service.Introduce;

namespace WebApi.Controllers.Admin
{
    [Route("api/admin/[controller]/[action]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatbotService _chatbot;

        public ChatController(ChatbotService chatbot)
        {
            _chatbot = chatbot;
        }

        //[HttpPost("tuvan")]
        //public async Task<IActionResult> TuVan([FromBody] string message)
        //{
        //    if (string.IsNullOrWhiteSpace(message))
        //        return BadRequest("Câu hỏi không hợp lệ.");

        //    var result = await _chatbot.GetTuVanAsync(message);
        //    return Ok(new { response = result });
        //}


    }
}

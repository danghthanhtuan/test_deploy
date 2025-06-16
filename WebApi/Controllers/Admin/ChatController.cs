using Microsoft.AspNetCore.Mvc;
using WebApi.DTO;
using WebApi.Service.Introduce;

namespace WebApi.Controllers.Admin
{
    [Route("api/introduce/[controller]/[action]")]

    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatbotService _chatbot;

        public ChatController(ChatbotService chatbot)
        {
            _chatbot = chatbot;
        }

        [HttpPost]
        public async Task<IActionResult> GetAdvice([FromBody] ChatRequestDTO dto)
        {
            var reply = await _chatbot.GetAdviceViaHttpClientAsync(dto.Message);
            return Ok(new { success = true, reply });
        }


    }
}

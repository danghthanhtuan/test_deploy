using OpenAI;
using OpenAI.Chat;
using Org.BouncyCastle.Asn1.Crmf;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Service.Introduce
{
    public class ChatbotService
    {
        private readonly OpenAIClient _client;

        public ChatbotService(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API Key không hợp lệ");

            _client = new OpenAIClient(apiKey);
        }

        //public async Task<string> GetTuVanAsync(string message)
        //{
        //    var chatMessages = new List<Message>
        //    {
        //        new Message(Role.System, "Bạn là trợ lý tư vấn cho công ty ABC. Trả lời ngắn gọn, rõ ràng và chuyên nghiệp."),
        //        new Message(Role.User, message)
        //    };

        //    var chatRequest = new ChatRequest(chatMessages, "gpt-3.5-turbo");

        //    var response = await _client.ChatEndpoint.GetCompletionAsync(chatRequest);

        //    return response.Choices.First().Message.Content;
        //}
    }
}

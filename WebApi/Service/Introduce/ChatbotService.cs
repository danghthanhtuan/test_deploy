using Microsoft.EntityFrameworkCore;
using WebApi.DTO;
using WebApi.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace WebApi.Service.Introduce
{
    public class ChatbotService
    {
        private readonly ManagementDbContext _context;
        private readonly HttpClient _client;

        public ChatbotService(ManagementDbContext context)
        {
            _context = context;
            _client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:1234")
            };
        }

        public async Task<string> GetAdviceViaHttpClientAsync(string userMessage)
        {
            // 1. Lấy dữ liệu dịch vụ từ DB
            var query = from st in _context.ServiceTypes
                        join r in _context.Regulations on st.ServiceGroupid equals r.ServiceGroupid
                        join g in _context.ServiceGroups on st.ServiceGroupid equals g.ServiceGroupid
                        select new ServiceTypeDTO1
                        {
                            Id = st.Id,
                            ServiceTypeNames = st.ServiceTypename,
                            Descriptionsr = st.DescriptionSr,
                            GroupName = g.GroupName,
                            Price = r.Price
                        };

            var danhSach = await query.ToListAsync();

            var moTa = string.Join("\n", danhSach.Select(d =>
                $"- {d.ServiceTypeNames} ({d.GroupName}): {d.Descriptionsr}. Giá: {d.Price:N0}đ"));

            // 2. Gửi prompt đến mô hình Local
            var requestData = new
            {
                model = "mistral-7b-instruct-v0.1.Q4_K_M.gguf",
                messages = new[]
                {
                    new {
                        role = "system",
                        content = $"Bạn là tư vấn viên chuyên nghiệp. Dưới đây là các dịch vụ:\n{moTa}"
                    },
                    new {
                        role = "user",
                        content = userMessage
                    }
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Local LLM error: {error}");
            }

            var resultJson = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(resultJson);
            return result.choices[0].message.content.ToString();
        }
    }
}

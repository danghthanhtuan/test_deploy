using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using WebApi.DTO;
using WebApi.Models;

public class ChatbotService
{
    private readonly ManagementDbContext _context;
    private readonly HttpClient _client;

    public ChatbotService(ManagementDbContext context)
    {
        _context = context;
        _client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:11434") // Ollama mặc định
        };
    }

    public async Task<string> GetAdviceViaHttpClientAsync(string userMessage)
    {
        // 1. Lấy dữ liệu từ DB
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

        // 2. Gửi prompt lên Ollama
        var requestData = new
        {
            model = "llama3", // Tên mô hình bạn đã chạy bằng `ollama run llama3`
            messages = new[]
            {
                new {
                    role = "system",
                    content = $"Bạn là một tư vấn viên chuyên nghiệp. Luôn luôn trả lời bằng tiếng Việt. Dưới đây là danh sách dịch vụ:\n{moTa}"
                },
                new {
                    role = "user",
                    content = userMessage
                }
            },
            stream = false // để nhận về toàn bộ phản hồi một lần
        };

        var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/chat", content); // Ollama endpoint đúng

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Local LLM error: {error}");
        }

        var resultJson = await response.Content.ReadAsStringAsync();
        dynamic result = JsonConvert.DeserializeObject(resultJson);
        return result.message.content.ToString();
    }
}

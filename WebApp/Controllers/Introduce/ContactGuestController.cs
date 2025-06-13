using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using WebApp.Configs;
using WebApp.DTO;

namespace WebApp.Controllers.Introduce
{
    public class ContactGuestController : Controller
    {
        private readonly ApiConfigs _apiConfigs;
        private readonly HttpClient _httpClient;

        // Inject IOptions<SmtpConfig>
        public ContactGuestController(IOptions<ApiConfigs> apiConfigs)
        {
            _apiConfigs = apiConfigs.Value;
            _httpClient = new HttpClient();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateContact([FromBody] CreateContactDTO model)
        {
            if (model == null || string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.Phone))
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }
            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync(_apiConfigs.BaseApiUrl + "/client/Contact/CreateContact", jsonContent).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return Ok(new { success = true, message = "Tạo liên hệ thành công!", data = responseData });
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return BadRequest(new { success = false, message = errorMessage });
                }
            }
            catch
            {
                return StatusCode(500, new { success = false, message = "Lỗi kết nối đến server." });
            }
        }
    }
}

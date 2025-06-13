using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using WebApp.Configs;
using WebApp.DTO;

namespace WebApp.Areas.Admin.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ApiConfigs _apiConfigs;
        private readonly HttpClient _httpClient;

        public NotificationController(IOptions<ApiConfigs> apiConfigs)
        {
            _apiConfigs = apiConfigs.Value;
            _httpClient = new HttpClient();
        }

        [HttpPost]
        public async Task<IActionResult> GetListNotification([FromBody] GetListNotificationPaging model)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            HttpResponseMessage response = _httpClient.PostAsync(_apiConfigs.BaseApiUrl + "/admin/Notification/GetList", jsonContent).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                return Ok(new { success = true, data = responseData });
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return BadRequest(new { success = false, message = errorMessage });
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using WebApp.Configs;
using WebApp.DTO;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/Notification")]
    public class NotificationController : Controller
    {
        private readonly ApiConfigs _apiConfigs;
        private readonly HttpClient _httpClient;

        public NotificationController(IOptions<ApiConfigs> apiConfigs)
        {
            _apiConfigs = apiConfigs.Value;
            _httpClient = new HttpClient();
        }

        [HttpPost("GetListNotification")]
        public async Task<IActionResult> GetListNotification([FromBody] GetListNotificationPaging model)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync(_apiConfigs.BaseApiUrl + "/admin/Notification/GetList", jsonContent).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<ListNotificationResponse>(responseData);
                return Ok(new { success = true, data = data.data });
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return BadRequest(new { success = false, message = errorMessage });
            }

        }

        [HttpPost("UpdateRead")]
        public async Task<IActionResult> UpdateRead([FromBody] UpdateReadNotificationReq req)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(req), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_apiConfigs.BaseApiUrl + "/admin/Notification/UpdateRead", jsonContent);

            if (response.IsSuccessStatusCode)
                return Ok(new { success = true });
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return BadRequest(new { success = false, message = errorMessage });
            }
        }
    }
}

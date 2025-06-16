using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using WebApp.Configs;
using WebApp.DTO;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/contact")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]
    public class ContactController : Controller
    {
        private readonly ApiConfigs _apiConfigs;
        private readonly HttpClient _httpClient;

        public ContactController(IOptions<ApiConfigs> apiConfigs)
        {
            _apiConfigs = apiConfigs.Value;
            _httpClient = new HttpClient();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("GetListContact")]
        public async Task<IActionResult> GetListContact([FromBody] GetListContactPaging model)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_apiConfigs.BaseApiUrl + "/admin/AdminContact/GetList", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<ListContactResponse>(responseData);
                return Ok(new { success = true, data = data.data });
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return BadRequest(new { success = false, message = errorMessage });
            }
        }

        [HttpGet("GetContactById")]
        public async Task<IActionResult> GetContactById([FromQuery] int id)
        {
            var response = await _httpClient.GetAsync(_apiConfigs.BaseApiUrl + $"/admin/AdminContact/GetContactById?id={id}");

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<ContactModelRes>(responseData);
                return Ok(new { success = true, data = data });
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return BadRequest(new { success = false, message = errorMessage });
            }
        }

        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateContactReq model)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync(_apiConfigs.BaseApiUrl + "/admin/AdminContact/UpdateStatus", jsonContent).Result;

            if (response.IsSuccessStatusCode)
            {
                return Ok(new { success = true });
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return BadRequest();
            }
        }
    }

}
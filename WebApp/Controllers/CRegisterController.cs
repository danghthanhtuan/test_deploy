using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using WebApp.DTO;

namespace WebApp.Controllers
{
    public class CRegisterController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7190/api/client");
        private readonly HttpClient _client;

        public CRegisterController()
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterclientDTO model)
        {
            if (model == null)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }
            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

                // Gửi request đến API backend với đường dẫn đúng
                HttpResponseMessage response = _client.PostAsync(_client.BaseAddress + "/CRegister/Register", jsonContent).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return Ok(new { success = true, message = "Đăng ký thành công!", data = responseData });
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

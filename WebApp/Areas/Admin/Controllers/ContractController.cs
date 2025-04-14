using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using WebApp.DTO;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/contract")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]
    public class ContractController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7190/api/admin");
        private readonly HttpClient _client;

        public ContractController()
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
        }

        

        [AuthorizeToken]
        [Route("")]
        public IActionResult Index()
        {
            if (User.IsInRole("QuanLy"))
            {
                return RedirectToAction("Index", "phanquyen");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IActionResult> Insert([FromBody] ContractDTO contractDTO, [FromQuery] string id)
        {
            if(!Request.Headers.ContainsKey("Authorization"))
                return Unauthorized(new {success = false, message = "Thiếu token"});
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer ","").Trim();
            if(string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { success = false, message = "Token không hợp lệ" });
            }
            if(string.IsNullOrEmpty(contractDTO.CustomerId))
            {
                return BadRequest(new { success = false, message = "Mã khách hàng không hợp lệ" }); 
            }
            if(contractDTO == null)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ"});
            }  
            try
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(contractDTO), Encoding.UTF8, "application/json");
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _client.PostAsync(_client.BaseAddress + $"/Contract/Insert?id={id}", jsonContent);
                var result = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<JObject>(result);
                string errorMessage = apiResponse["message"]?.ToString() ?? "Có lỗi xảy ra từ API";
                if(response.IsSuccessStatusCode)
                {
                    return Ok(new { success = true, message = errorMessage });
                }
                else
                {
                    return BadRequest(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {success = false , message = "Lỗi hệ thống, vui lòng thử lại sau"});
            }
        }
        
    }
}

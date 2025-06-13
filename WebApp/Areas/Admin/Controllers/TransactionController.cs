using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using WebApp.DTO;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/Transaction")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]
    public class TransactionController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7190/api/admin");
        private readonly HttpClient _client;

        public TransactionController()
        {
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromMinutes(5); // Thêm dòng này
            _client.BaseAddress = baseAddress;
        }

        [AuthorizeToken]
        [Route("")]
        public IActionResult Index()
        {
            if (User.IsInRole("HanhChinh") || User.IsInRole("KyThuat"))
            {
                return RedirectToAction("Index", "phanquyen");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        [Route("GetAllCompany")]
        public async Task<IActionResult> GetAllCompany([FromBody] GetListTransactionPaging req)
        {
            try
            {
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { success = false, message = "Thiếu token." });

                List<PaymentTransactionDTO> listTran = new List<PaymentTransactionDTO>();
                var reqjson = JsonConvert.SerializeObject(req);
                var httpContent = new StringContent(reqjson, Encoding.UTF8, "application/json");

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await _client.PostAsync(_client.BaseAddress + "/transaction/GetAllCompany", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<PagingResult<PaymentTransactionDTO>>(responseData);
                    return Ok(new { success = true, listTran = responseObject });
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

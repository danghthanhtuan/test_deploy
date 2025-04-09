using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        [Route("GetAllCompany")]
        public async Task<IActionResult> GetAllCompany([FromBody] GetListCompanyPaging req)
        {
            try
            {
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { success = false, message = "Thiếu token." });

                List<CompanyAccountDTO> listCompany = new List<CompanyAccountDTO>();
                var reqjson = JsonConvert.SerializeObject(req);
                var httpContent = new StringContent(reqjson, Encoding.UTF8, "application/json");

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await _client.PostAsync(_client.BaseAddress + "/Account/GetAllCompany", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<PagingResult<CompanyAccountDTO>>(responseData);
                    return Ok(new { success = true, listCompany = responseObject });
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

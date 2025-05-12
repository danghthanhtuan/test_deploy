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
    [Route("admin/request")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]

    public class RequestController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7190/api/admin");
        private readonly HttpClient _client;

        public RequestController()
        {
            _client = new HttpClient();
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
        //[AuthorizeToken]
        //[Route("")]
        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpPost]
        [Route("GetAllRequest")]
        public async Task<IActionResult> GetAllRequest([FromBody] GetListReq req)
        {
            try
            {
                
                List<Requirement_Company> listReq = new List<Requirement_Company>();
                var reqjson = JsonConvert.SerializeObject(req);
                var httpContent = new StringContent(reqjson, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync(_client.BaseAddress + "/Request/GetAllRequest", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<PagingResult<Requirement_Company>>(responseData);
                    return Ok(new { success = true, listRequest = responseObject });
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
        [HttpGet]
        [Route("GetAllInfor")]
        public async Task<IActionResult> GetAllInfor([FromQuery] string customerID)
        {
            try
            {
                List<CompanyAccountDTO> listRequest = new List<CompanyAccountDTO>();

                HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + $"/Request/GetAllInfor?req={customerID}");

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<List<CompanyAccountDTO>>(responseData);
                    return Ok(new { success = true, listRequest = responseObject });
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

        [HttpPost]
        [Route("Insert")]
        public async Task<IActionResult> Insert([FromBody] Requirement_C Req, [FromQuery] string id)
        {
            // Lấy token từ header Authorization
            if (!Request.Headers.ContainsKey("Authorization"))
                return Unauthorized(new { success = false, message = "Thiếu token." });

            string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { success = false, message = "Token không hợp lệ." });

            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { success = false, message = "Mã nhân viên không hợp lệ!" });

            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(Req.ContractNumber))
                return BadRequest(new { success = false, message = "Mã khách hàng không hợp lệ!" });

            if (Req == null)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            try
            {
                // Chuẩn bị request body
                var jsonContent = new StringContent(JsonConvert.SerializeObject(Req), Encoding.UTF8, "application/json");
                // Gửi request với token
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _client.PostAsync(_client.BaseAddress + $"/Request/Insert?id={id}", jsonContent);

                var result = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<JObject>(result);

                // Lấy message dưới dạng string, tránh lỗi mảng rỗng
                string errorMessage = apiResponse["message"]?.ToString() ?? "Có lỗi xảy ra từ API.";

                if (response.IsSuccessStatusCode)
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
                Console.WriteLine($"Lỗi hệ thống: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống, vui lòng thử lại sau." });
            }
        }

        [HttpGet]
        [Route("GetRequestByID")]
        public async Task<IActionResult> GetRequestByID([FromQuery] string requestID)
        {
            try
            {
                List<Requirement_Company> listRequest = new List<Requirement_Company>();

                HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + $"/Request/GetRequestByID?req={requestID}");

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<List<Requirement_Company>>(responseData);
                    return Ok(new { success = true, listRequest = responseObject });
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

        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus([FromBody] historyRequest historyReq)
        {
            try
            {
                var reqjson = JsonConvert.SerializeObject(historyReq);
                var httpContent = new StringContent(reqjson, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PutAsync(_client.BaseAddress + "/Request/UpdateStatus", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "Cập nhật thành công!",
                        data = responseData
                    });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    // Deserialize chuỗi JSON lỗi từ backend để lấy message
                    string message = "Có lỗi xảy ra.";
                    try
                    {
                        dynamic parsedError = JsonConvert.DeserializeObject(errorContent);
                        message = parsedError?.message ?? message;
                    }
                    catch
                    {
                        message = errorContent; // Nếu không parse được thì trả nguyên nội dung
                    }

                    return BadRequest(new
                    {
                        success = false,
                        message = message
                    });
                }
            }
            catch
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi kết nối đến server."
                });
            }
        }


        [HttpGet]
        [Route("getHIS")]
        public async Task<IActionResult> getHIS([FromQuery] string requestID)
        {
            try
            {
                List<HistoryRequests> listRequest = new List<HistoryRequests>();

                HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + $"/Request/getHIS?req={requestID}");

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<List<HistoryRequests>>(responseData);
                    return Ok(new { success = true, listRequest = responseObject });
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

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using WebApp.Models;
using WebApp.DTO;
using System.Text.Json;
using Azure;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using static WebApp.Controllers.LoginclientController;
using WebApp.Configs;
using Microsoft.Extensions.Options;


namespace WebApp.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin")]
    [Route("admin/LoginAdmin")]

    public class LoginAdmin : Controller
    {
        private readonly HttpClient _client;
        private readonly ApiConfigs _apiConfigs;

        public LoginAdmin(IOptions<ApiConfigs> apiConfigs)
        {
            _client = new HttpClient();
            _apiConfigs = apiConfigs.Value;

        }

        [Route("")]
        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login");
            else
                return RedirectToAction("Index", "homeadmin");
        }

        [AuthorizeToken]
        [Route("Login")]
        public IActionResult Login()
        {
            return View();
        }

        [AuthorizeToken]
        [Route("ResetPass")]
        public IActionResult ResetPass()
        {
            return View();
        }

        [HttpPost]
        [Route("LoginAuthenticate")]
      
        public async Task<IActionResult> LoginAuthenticate([FromBody] LoginRequest model)
        {
            if (model == null)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync(_apiConfigs.BaseApiUrl + "/admin/Home/Login", jsonContent);
                string dataJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest(new { success = false, message = "Lỗi đăng nhập: " + dataJson });
                }

                var apiResponse = JsonConvert.DeserializeObject<APIResponse<Staff>>(dataJson);

                if (apiResponse == null || !apiResponse.Success || apiResponse.Data == null)
                {
                    return BadRequest(new { success = false, message = apiResponse?.Message ?? "Sai tên đăng nhập hoặc mật khẩu." });
                }

                //// Gán quyền
                //string role = apiResponse.Data.Staffphone switch
                //{
                //    "0123654789" => "QuanLy",
                //    "0365812847" => "Admin",
                //    _ => "User"
                //};

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, apiResponse.Data.Staffname),
                    new Claim(ClaimTypes.Role, apiResponse.Data.Department), 
                    new Claim("StaffId", apiResponse.Data.Staffid),
                    new Claim("Department", apiResponse.Data.Department)
                };

                var claimsIdentity = new ClaimsIdentity(claims, "AdminCookie");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync("AdminCookie", claimsPrincipal);

                Response.Cookies.Append("AuthToken", apiResponse.Message, new CookieOptions
                {
                    HttpOnly = true,  // Cookie chỉ được gửi qua HTTP, không thể truy cập bằng JavaScript
                    Secure = true,    // Chỉ gửi cookie qua HTTPS
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(5)
                });


                // Trả về accessToken thay vì message
                return Ok(new { success = true, accessToken = apiResponse.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }



        [Route("Logout")]
        //[Authorize(AuthenticationSchemes = "AdminCookie")]
        public async Task<IActionResult> Logout()
        {
            // Xóa phiên đăng nhập
            await HttpContext.SignOutAsync("AdminCookie");

            return RedirectToAction("Login");
        }
        [HttpPost]
        [Route("SendEmailOTP")]
        public async Task<IActionResult> SendEmailOTP([FromBody] SendOtpRequest request)
        {
            Console.WriteLine($"Received: phone={request.phoneNumber}, email={request.userEmail}");

            if (request == null || string.IsNullOrEmpty(request.phoneNumber) || string.IsNullOrEmpty(request.userEmail))
            {
                return Json(new { success = false, message = "Thiếu dữ liệu!" });
            }

            var requestData = new { phoneNumber = request.phoneNumber, userEmail = request.userEmail };
            HttpResponseMessage response = await _client.PostAsJsonAsync(_apiConfigs.BaseApiUrl + "/admin/Home/SendEmail_OTP", requestData);

            if (response.IsSuccessStatusCode)
            {
                string dataJson = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<APIResponse<object>>(dataJson);
                return Json(new { success = apiResponse.Success, message = apiResponse.Message });
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return Json(new { success = false, message = responseContent });
        }

        [HttpGet]
        [Route("CheckEmailRegister")]
        public IActionResult CheckEmailRegister(string phoneNumber, string otp)
        {
            HttpResponseMessage response = _client.GetAsync(_apiConfigs.BaseApiUrl + $"/admin/Home/CheckEmail_Register/{phoneNumber}/{otp}").Result;

            if (response.IsSuccessStatusCode)
            {
                string dataJson = response.Content.ReadAsStringAsync().Result;
                var apiResponse = JsonConvert.DeserializeObject<APIResponse<object>>(dataJson);

                if (apiResponse != null && apiResponse.Success)
                {
                    return Json(new { success = true, message = apiResponse.Message });
                }
                else
                {
                    return Json(new { success = false, message = apiResponse.Message });
                }
            }
            else
            {
                return Json(new { success = false, message = response.Content.ReadAsStringAsync() });
            }
        }

        [HttpPost]
        [Route("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword([FromBody] LoginRequest model)
        {
            if (model == null)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }
            try
            {
                var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync(_apiConfigs.BaseApiUrl + "/admin/Home/UpdatePassword", jsonContent);

                var responseData = await response.Content.ReadAsStringAsync();

                // Parse response để lấy message chính xác
                var responseObj = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(responseData);

                if (response.IsSuccessStatusCode)
                {
                    string message = responseObj.TryGetProperty("message", out var msg) ? msg.GetString() : "Cập nhật mật khẩu thành công!";
                    return Ok(new { success = true, message = message });
                }
                else
                {
                    string errorMessage = responseObj.TryGetProperty("message", out var msg) ? msg.GetString() : "Cập nhật mật khẩu thất bại!";
                    return BadRequest(new { success = false, message = errorMessage });
                }
            }
            catch
            {
                return StatusCode(500, new { success = false, message = "Lỗi kết nối đến server." });
            }
        }
        public class SendOtpRequest
        {
            public string phoneNumber { get; set; }
            public string userEmail { get; set; }
        }
    }
}

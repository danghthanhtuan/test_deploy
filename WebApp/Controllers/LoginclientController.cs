using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using WebApp.DTO;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class LoginclientController : Controller
    {

        Uri baseAddress = new Uri("https://localhost:7190/api/client");
        private readonly HttpClient _client;

        public LoginclientController()
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;

        }

        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Loginclient");
            else
                return RedirectToAction("Index", "home");
        }

        public IActionResult Loginclient()
        {
            return View();
        }

        public IActionResult ResetPass()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginAuthenticate([FromBody] LoginRequest model)
        {
            if (model == null)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            try
            {
                var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
                // Gửi request đến API backend
                HttpResponseMessage response = _client.PostAsync(_client.BaseAddress + "/Login/Login", jsonContent).Result;

                string dataJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    TempData["error"] = "Lỗi đăng nhập: " + dataJson;
                    return View("Loginclient");
                }

                var apiResponse = JsonConvert.DeserializeObject<APIResponse<Account>>(dataJson);

                if (apiResponse == null || !apiResponse.Success || apiResponse.Data == null)
                {
                    TempData["error"] = apiResponse?.Message ?? "Lỗi không xác định từ API.";
                    return View("Loginclient");
                }

                // Tạo danh sách Claims
                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, apiResponse.Data.Rootname),
                        new Claim("CustomerId", apiResponse.Data.Customerid.ToString())
                    };

                var claimsIdentity = new ClaimsIdentity(claims, "User");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync("User", claimsPrincipal);

                //  return RedirectToAction("Index", "Home"); // Chuyển hướng sau khi đăng nhập thành công
                return Ok(new { success = true, message = "Đăng nhập thành công." });
            }
            catch (Exception ex)
            {
                TempData["error"] = "Lỗi hệ thống: " + ex.Message;
                return View("Loginclient");
            }
        }


        [Authorize(AuthenticationSchemes = "User")]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Xóa phiên đăng nhập
            await HttpContext.SignOutAsync("User");

            return RedirectToAction("Loginclient", "Loginclient");
        }

        [HttpPost]
        public async Task<IActionResult> SendEmailOTP([FromBody] SendOtpRequest request)
        {
            Console.WriteLine($"Received: phone={request.phoneNumber}, email={request.userEmail}");

            if (request == null || string.IsNullOrEmpty(request.phoneNumber) || string.IsNullOrEmpty(request.userEmail))
            {
                return Json(new { success = false, message = "Thiếu dữ liệu!" });
            }

            var requestData = new { phoneNumber = request.phoneNumber, userEmail = request.userEmail };
            HttpResponseMessage response = await _client.PostAsJsonAsync(_client.BaseAddress + "/Login/SendEmail_OTP", requestData);

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
        public IActionResult CheckEmailRegister(string phoneNumber, int otp)
        {
            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + $"/Login/CheckEMmail_Register/{phoneNumber}/{otp}").Result;

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

        //[HttpPost]
        //public async Task<IActionResult> UpdatePassword([FromBody] LoginRequest model)
        //{
        //    if (model == null)
        //    {
        //        return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
        //    }
        //    try
        //    {
        //        var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

        //        // Gửi request đến API backend với đường dẫn đúng
        //        HttpResponseMessage response = _client.PostAsync(_client.BaseAddress + "/Login/UpdatePassword", jsonContent).Result;

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var responseData = await response.Content.ReadAsStringAsync();
        //            return Ok(new { success = true, message = "Cập nhật mật khẩu mới thành công!", data = responseData });
        //        }
        //        else
        //        {
        //            var errorMessage = await response.Content.ReadAsStringAsync();
        //            return BadRequest(new { success = false, message = errorMessage });
        //        }
        //    }
        //    catch
        //    {
        //        return StatusCode(500, new { success = false, message = "Lỗi kết nối đến server." });
        //    }
        //}
        [HttpPost]
        public async Task<IActionResult> UpdatePassword([FromBody] LoginRequest model)
        {
            if (model == null)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }
            try
            {
                var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync(_client.BaseAddress + "/Login/UpdatePassword", jsonContent);

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

        // DTO cho dữ liệu nhận vào
        public class SendOtpRequest
        {
            public string phoneNumber { get; set; }
            public string userEmail { get; set; }
        }

    }

}
    


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


namespace WebApp.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin")]
    [Route("admin/LoginAdmin")]

    public class LoginAdmin : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7190/api/admin");
        private readonly HttpClient _client;

        public LoginAdmin()
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;

        }

        [Route("")]
        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login");
            else
                return RedirectToAction("Index", "homeadmin");
        }

        //[Route("")]
        //public ActionResult Index()
        //{
        //    if (!User.Identity.IsAuthenticated)
        //        return View("Login"); // Hiển thị trang Login trực tiếp thay vì redirect
        //    return RedirectToAction("Index", "homeadmin");
        //}

        [AuthorizeToken]
        [Route("Login")]
        public IActionResult Login()
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
                HttpResponseMessage response = await _client.PostAsync(_client.BaseAddress + "/Home/Login", jsonContent);
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

                // Gán quyền
                string role = apiResponse.Data.Staffphone switch
                {
                    "0123654789" => "QuanLy",
                    "0365812847" => "Admin",
                    _ => "User"
                };

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, apiResponse.Data.Staffname),
                    new Claim("StaffId", apiResponse.Data.Staffid.ToString()),
                    new Claim(ClaimTypes.Role, role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, "AdminCookie");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync("AdminCookie", claimsPrincipal);


                // Lưu token vào cookie
                // Lưu token vào Cookie với HttpOnly
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
    }
}

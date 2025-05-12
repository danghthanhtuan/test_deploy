using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using WebApp.DTO;
using WebApp.Models;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/staff")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]
    public class StaffController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7190/api/admin");
        private readonly HttpClient _client;
        public StaffController()
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
        [HttpGet]
        [Route("GetAllNhanVien")]
        public ActionResult GetAllNhanVien()
        {
            try
            {
                HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/Staff/GetAllNhanVien").Result;

                if (response.IsSuccessStatusCode)
                {
                    string dataJson = response.Content.ReadAsStringAsync().Result;
                    var apiResponse = JsonConvert.DeserializeObject<APIResponse<List<Staff>>>(dataJson);

                    if (apiResponse != null && apiResponse.Success)
                    {
                        return Json(new { success = true, data = apiResponse.Data });
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
            catch (Exception ex)
            {
                // Trong trường hợp có lỗi, có thể log lỗi hoặc xử lý theo nhu cầu của bạn
                return Json(new { Error = ex.Message });
            }
        }
        [Route("ThongTinNhanVien")]
        [HttpPost]
        public ActionResult ThongTinNhanVien(string id)
        {
            try
            {
                HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + $"/Staff/GetById/{id}").Result;

                if (response.IsSuccessStatusCode)
                {
                    string dataJson = response.Content.ReadAsStringAsync().Result;
                    var apiResponse = JsonConvert.DeserializeObject<APIResponse<StaffDTO>>(dataJson);

                    if (apiResponse != null && apiResponse.Success)
                    {
                        return Json(new { success = true, data = apiResponse.Data });
                    }
                    else
                    {
                        return Json(new { success = false, message = apiResponse.Message });
                    }
                }
                else
                {
                    // Trả về thông báo lỗi nếu không tìm thấy sách
                    return Json(new { success = false, message = response.Content.ReadAsStringAsync() });
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                Console.WriteLine($"Error in ChiTietDocGia: {ex.Message}");
                return Json(new { success = false, message = "Đã xảy ra lỗi" });
            }
        }
        [HttpPost]
        [Route("ThemNhanVien")]
        public async Task<IActionResult> ThemNhanVien([FromBody] StaffDTO obj)
        {
                // Lấy token từ header Authorization
            if (!Request.Headers.ContainsKey("Authorization"))
                return Unauthorized(new { success = false, message = "Thiếu token." });

            string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { success = false, message = "Token không hợp lệ." });

            if (obj == null)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            try
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _client.PostAsync(_client.BaseAddress + "/staff/ThemNhanVien", jsonContent);

                var result = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<JObject>(result);

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


        [HttpPost]
        [Route("CapNhatThongTin")]
        public async Task<IActionResult> CapNhatThongTin([FromBody] StaffDTO obj)
        {
            // Lấy token từ header Authorization
            if (!Request.Headers.ContainsKey("Authorization"))
                return Unauthorized(new { success = false, message = "Thiếu token." });

            string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { success = false, message = "Token không hợp lệ." });

            if (obj == null)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            try
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _client.PostAsync(_client.BaseAddress + "/staff/UpdateThongTinNhanVien", jsonContent);

                var result = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<JObject>(result);

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
                // Kiểm tra xem lỗi có phải do tồn tại số điện thoại không
                if (ex.Message.Contains("Số điện thoại đã tồn tại."))
                    return Json(new { success = false, message = "Số điện thoại đã tồn tại." });

                // Kiểm tra xem lỗi có phải do tồn tại số username không
                if (ex.Message.Contains("Username đã tồn tại."))
                    return Json(new { success = false, message = "Username đã tồn tại." });

                // Xử lý lỗi nếu có
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Đã xảy ra lỗi" });
            }
        }
    }
}

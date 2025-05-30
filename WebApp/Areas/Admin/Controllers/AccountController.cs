using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Ocsp;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebApp.DTO;
using WebApp.Models;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/account")]
    [Authorize(AuthenticationSchemes = "AdminCookie")]
    public class AccountController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7190/api/admin");
        private readonly HttpClient _client;

        public AccountController()
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
                HttpResponseMessage response = await _client.PostAsync(_client.BaseAddress + "/account/GetAllCompany", httpContent);

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


        [HttpPut]
        [Route("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus([FromBody] updateID updateID)
        {
            try
            {
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { success = false, message = "Thiếu token." });

                var reqjson = JsonConvert.SerializeObject(updateID);
                var httpContent = new StringContent(reqjson, Encoding.UTF8, "application/json");
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Gửi request đến API backend với đường dẫn đúng
                HttpResponseMessage response = await _client.PutAsync(_client.BaseAddress + "/account/UpdateStatus", httpContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseData = response.Content.ReadAsStringAsync();
                    return Ok(new { success = true, message = "Cập nhật thành công!", data = responseData });
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
        [Route("Update")]
        public async Task<IActionResult> Update([FromBody] CompanyAccountDTO companyAccountDTO, [FromQuery] string id)
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

            if (companyAccountDTO == null)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            try
            {
                // Chuẩn bị request body
                var jsonContent = new StringContent(JsonConvert.SerializeObject(companyAccountDTO), Encoding.UTF8, "application/json");
                // Gửi request với token
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _client.PostAsync(_client.BaseAddress + $"/account/Update?id={id}", jsonContent);
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

        [HttpPost("ExportToCsv")]
        public async Task<IActionResult> ExportToCsv([FromBody] ExportRequestDTO request)
        {
            using (var httpClient = new HttpClient())
            {
                var reqJson = JsonConvert.SerializeObject(request);
                var jsonContent = new StringContent(reqJson, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync(_client.BaseAddress + "/account/ExportToCsv", jsonContent);
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest(new { success = false, message = "Xuất file thất bại!" });
                }

                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                return File(fileBytes, "text/csv", "DanhSachKhachHang.csv");
            }
        }

        [HttpGet]
        [Route("GetListServiceID")]
        public async Task<IActionResult> GetListServiceID()
        {
            try
            {
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { success = false, message = "Thiếu token." });

                List<ServiceTypeDTO2> listRegu = new List<ServiceTypeDTO2>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + "/account/GetListServiceID");

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<List<ServiceTypeDTO2>>(responseData);
                    return Ok(new { success = true, listRegu = responseObject });
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

        //lưu db +  chờ boss ký
        [HttpPost]
        [Route("GenerateContract")]
        public async Task<IActionResult> GenerateContract([FromBody] CompanyAccountDTO dto, [FromQuery] string id)
        {
            try
            {
                // Lấy token từ header để truyền qua API service
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _client.PostAsync(_client.BaseAddress + $"/account/GenerateContract?id={id}", content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Có thể trả về success + link để frontend xử lý
                    return Ok(new { success = true, message = "Hợp đồng đã được tạo và gửi email.", data = JsonConvert.DeserializeObject(result) });
                }
                else
                {
                    return BadRequest(new { success = false, message = result });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        [Route("GetListPending")]
        public async Task<IActionResult> GetListPending([FromBody] GetListCompanyPaging req)
        {
            try
            {
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { success = false, message = "Thiếu token." });

                List<CompanyContractDTOs> pendingContracts = new List<CompanyContractDTOs>();
                var reqjson = JsonConvert.SerializeObject(req);
                var httpContent = new StringContent(reqjson, Encoding.UTF8, "application/json");

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await _client.PostAsync(_client.BaseAddress + "/account/GetListPending", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<PagingResult<CompanyContractDTOs>>(responseData);
                    return Ok(new { success = true, pendingContracts = responseObject });
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

        //hàm boss ký. 
        [HttpPost]
        [Route("SignPdfWithAdminCertificate")]
        public async Task<IActionResult> SignPdfWithAdminCertificate([FromBody] SignAdminRequest request)
        {
            try
            {
                // Lấy token từ Header
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { success = false, message = "Thiếu token." });

                // Thiết lập Authorization Header
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Gửi request đến API ký hợp đồng
                var httpContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(_client.BaseAddress + "/account/SignPdfWithAdminCertificate", httpContent);

                // Đọc nội dung phản hồi từ API
                string responseBody = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<JObject>(responseBody);

                if (response.IsSuccessStatusCode)
                {
                    string signedFilePath = apiResponse["signedFilePath"]?.ToString();
                    return Ok(new { success = true, signedFilePath });
                }
                else
                {
                    string errorMessage = apiResponse["message"]?.ToString() ?? "Ký thất bại.";
                    return BadRequest(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi hệ thống: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống, vui lòng thử lại sau." });
            }
        }

        //gửi client
        [HttpPost]
        [Route("SendEmailtoclient")]
        public async Task<IActionResult> SendEmailtoclient([FromBody] SignAdminRequest dto)
        {
            try
            {
                // Lấy token từ header để truyền qua API service
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { success = false, message = "Thiếu token." });
                var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await _client.PostAsync(_client.BaseAddress + "/account/SendEmailtoclient", content);

               // var response = await _client.PostAsync(_client.BaseAddress + "/account/SendEmailtoclient", content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Có thể trả về success + link để frontend xử lý
                    return Ok(new { success = true, message = "Hợp đồng đã được gửi đến email thành công", data = JsonConvert.DeserializeObject(result) });
                }
                else
                {
                    return BadRequest(new { success = false, message = result });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IActionResult> Insert([FromBody] SignAdminRequest request)
        {
            // Lấy token từ header Authorization
            if (!Request.Headers.ContainsKey("Authorization"))
                return Unauthorized(new { success = false, message = "Thiếu token." });

            string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { success = false, message = "Token không hợp lệ." });

            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(request.StaffId))
                return BadRequest(new { success = false, message = "Mã nhân viên không hợp lệ!" });

            if (request == null)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });

            try
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _client.PostAsync(_client.BaseAddress + "/account/Insert", jsonContent);

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
    }
}

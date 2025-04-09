using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using WebApp.DTO;
using WebApp.Models;

namespace WebApp.Controllers
{
    
    [Authorize(AuthenticationSchemes = "User")]
    public class ClientController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7190/api/client");
        private readonly HttpClient _client;

        public ClientController()
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
        }

        public IActionResult Index()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> GetAllRequest([FromBody] GetListReq req)
        //{
        //    try
        //    {
        //        List<Requirement_Company> listRequest = new List<Requirement_Company>();
        //        var reqjson = JsonConvert.SerializeObject(req);
        //        var httpContent = new StringContent(reqjson, Encoding.UTF8, "application/json");

        //        HttpResponseMessage response = await _client.PostAsync(_client.BaseAddress + "/Requirements/GetAllRequest", httpContent);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var responseData = await response.Content.ReadAsStringAsync();
        //            var responseObject = JsonConvert.DeserializeObject<PagingResult<Requirement_Company>>(responseData);
        //            return Ok(new { success = true, listRequest = responseObject });
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

        [HttpGet]
        public async Task<IActionResult> GetAllInfor([FromQuery] string customerID)
        {
            try
            {
                List<CompanyAccountDTO> listRequest = new List<CompanyAccountDTO>();

                HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + $"/Requirements/GetAllInfor?req={customerID}");

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
        public async Task<IActionResult> Insert([FromBody] Requirement_C Req)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(Req.CustomerId))
                return BadRequest(new { success = false, message = "Mã khách hàng không hợp lệ!" });

            if (Req == null)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            try
            {


                // Chuẩn bị request body
                var jsonContent = new StringContent(JsonConvert.SerializeObject(Req), Encoding.UTF8, "application/json");

                var response = await _client.PostAsync(_client.BaseAddress + "/Requirements/Insert", jsonContent);

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
        public async Task<IActionResult> GetdetailRequest([FromQuery] string query)
        {
            try
            {
                List<Requirement_Company> listHis = new List<Requirement_Company>();
                HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + $"/Requirements/GetdetailRequest?query={query}");
                if (response.IsSuccessStatusCode)
                {
                    var reponseData = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<Requirement_Company>(reponseData);
                    return Ok(new { success = true, listHis = responseObject });

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
        public async Task<IActionResult> GetAllRequest([FromBody] GetListReq req)
        {
            try
            {
                PagingResult<Request_GroupCompany_DTO> phieuTraList = new PagingResult<Request_GroupCompany_DTO>();

                var reqjson = JsonConvert.SerializeObject(req);
                var httpContent = new StringContent(reqjson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(_client.BaseAddress + "/Requirements/GetAllRequest", httpContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<PagingResult<Request_GroupCompany_DTO>>(responseData);
                    return Ok(new { success = true, listRequest = responseObject });
                  
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return BadRequest(new { success = false, message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                return StatusCode(500, new { success = false, message = "Lỗi kết nối đến server." });
            }
            
        }
    }
}

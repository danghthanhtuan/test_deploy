using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using WebApp.Configs;
using WebApp.DTO;

namespace WebApp.Controllers.Introduce
{
    public class ServiceGuestController : Controller
    {
        
        private readonly ApiConfigs _apiConfigs;

        private readonly HttpClient _client;
        public ServiceGuestController(IOptions<ApiConfigs> apiConfigs)
        {
            _client = new HttpClient();
            _apiConfigs = apiConfigs.Value;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                HttpResponseMessage response = await _client.PostAsync(_apiConfigs.BaseApiUrl + "/introduce/ServiceGuest/GetAll", null);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<List<ServiceTypeDTO1>>(responseData);
                    
                    return View(responseObject); 
                }
                else
                {
                    ViewBag.Error = "Không lấy được dữ liệu từ API.";
                    return View(new List<ServiceTypeDTO1>());
                }
            }
            catch
            {
                ViewBag.Error = "Lỗi kết nối đến server.";
                return View(new List<ServiceTypeDTO1>());
            }
        }


        public async Task<IActionResult> GetAllRegulations([FromBody] GetListReq req)
        {
            try
            {
                
                List<ServiceTypeDTO1> listRegu = new List<ServiceTypeDTO1>();
                var reqjson = JsonConvert.SerializeObject(req);
                var httpContent = new StringContent(reqjson, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync(_apiConfigs.BaseApiUrl + "/introduce/ServiceGuest/GetAllRegulations", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<PagingResult<ServiceTypeDTO1>>(responseData);
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
    } 
}

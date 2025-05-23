using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using System.Net.Http;
using System.Text;
using WebApp.DTO;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace WebApp.Areas.Controllers
{
    public class SeeContractController : Controller
    {
        Uri baseAddress = new Uri("https://localhost:7190/api/admin");
        private readonly HttpClient _client;
        public SeeContractController()
        {
            _client = new HttpClient();
            _client.BaseAddress = baseAddress;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UploadSignedFile(IFormFile signedFile, string email, string originalFileName)
        {
            if (signedFile == null || signedFile.Length == 0 || originalFileName==null)
                return Json(new { success = false, message = "Vui lòng chọn file hợp lệ." });

            // Tạo HttpClient để gọi API Controller
            using (var client = new HttpClient())
            {
                var apiUrl = "https://localhost:7190/api/admin/SeeContract_Sign/UploadSignedFile";

                var form = new MultipartFormDataContent();
                form.Add(new StreamContent(signedFile.OpenReadStream()), "signedFile", signedFile.FileName);
                form.Add(new StringContent(email), "email");
                form.Add(new StringContent(originalFileName), "originalFileName"); // Thêm dòng này


                var response = await client.PostAsync(apiUrl, form);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return Json(new { success = true, message = "Gửi đến API thành công", result });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = "Lỗi từ API", error });
                }
            }
        }

    }
}

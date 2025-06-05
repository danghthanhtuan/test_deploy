// PaymentController.cs
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net.Http;
using WebApp.Helpers; // Đảm bảo bạn có VnPayLibrary ở đây

namespace WebApp.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public PaymentController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> ThanhToanVNPAY(decimal soTien, string mahopdong)
        {
            // 🧾 Bước 1: Gọi API tạo bản ghi Payment
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("https://localhost:7190/api/admin/payment/CreatePayment", new
            {
                SoTien = soTien,
                MaHopDong = mahopdong,
            });

            if (!response.IsSuccessStatusCode)
                return BadRequest("Không tạo được đơn thanh toán");

            var responseData = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseData);
            string paymentId = json["id"].ToString();

            // 🔗 Bước 2: Tạo link thanh toán VNPAY
            return ThanhToan(paymentId, soTien); // Không cần `Redirect(...)` nữa
        }

        [HttpPost]
        public IActionResult ThanhToan(string paymentId, decimal amount)
        {
            var vnp_Returnurl = _configuration["VNPAY:vnp_Returnurl"];
            var vnp_Url = _configuration["VNPAY:vnp_Url"];
            var vnp_TmnCode = _configuration["VNPAY:vnp_TmnCode"];
            var vnp_HashSecret = _configuration["VNPAY:vnp_HashSecret"];

            if (string.IsNullOrEmpty(vnp_TmnCode) || string.IsNullOrEmpty(vnp_HashSecret))
            {
                ViewBag.Message = "Thiếu thông tin cấu hình VNPAY";
                return View("Error"); // OK vì giờ return IActionResult
            }

            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((int)(amount * 100)).ToString());
            vnpay.AddRequestData("vnp_BankCode", "VNBANK");
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don #" + paymentId);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", paymentId);

            var url = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            Console.WriteLine("URL redirect đến VNPAY: " + url);

            return Redirect(url); // Chuyển hướng luôn từ đây
        }

        [HttpGet]
        public async Task<IActionResult> KetQuaThanhToan()
        {
            var vnpay = new VnPayLibrary();
            var response = Request.Query;

            foreach (var key in response.Keys)
            {
                Console.WriteLine($"{key} = {response[key]}");
                //vnpay.AddResponseData(key, response[key]);
                vnpay.AddResponseData(key, response[key].ToString().Trim());

            }

            string vnp_HashSecret = _configuration["VNPAY:vnp_HashSecret"];
            bool checkSignature = vnpay.ValidateSignature(response["vnp_SecureHash"], vnp_HashSecret);

            string transactionStatus = response["vnp_TransactionStatus"];
            string responseCode = response["vnp_ResponseCode"];
            string id = response["vnp_TxnRef"];
            string maGiaoDich = response["vnp_TransactionNo"];
            string fullInfo = response["vnp_OrderInfo"];

            string email = "";
            if (!string.IsNullOrEmpty(fullInfo) && fullInfo.Contains("Email:"))
            {
                email = fullInfo.Split("Email:")[1].Trim();
            }

            string paymentMethod = "VNPAY";
            string tinhTrang = (transactionStatus == "00" && responseCode == "00") ? "Thanh cong" : "That bai";

            if (!checkSignature)
            {
                tinhTrang = "Sai chữ ký"; // ❗ Cho biết là lỗi chữ ký
            }

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var requestBody = new
                    {
                        ID = id,
                        MaGiaoDich = maGiaoDich,
                        PhuongThuc = paymentMethod,
                        TinhTrang = tinhTrang
                    };

                    var responseApi = await httpClient.PostAsJsonAsync("https://localhost:7190/api/admin/payment/CapNhatThanhToan", requestBody);

                    if (responseApi.IsSuccessStatusCode)
                    {
                        if (checkSignature && tinhTrang == "Thanh cong")
                            return View("KetQuaThanhToanThanhCong");
                        else
                        {
                            ViewBag.Message = "Thanh toán thất bại hoặc sai chữ ký.";
                            return View("Error");
                        }
                    }
                    else
                    {
                        string err = await responseApi.Content.ReadAsStringAsync();
                        ViewBag.Message = $"Kết nối thành công nhưng cập nhật thất bại: {err}";
                        return View("Error");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Lỗi hệ thống khi gửi dữ liệu thanh toán: {ex.Message}";
                return View("Error");
            }
        }

    }
}
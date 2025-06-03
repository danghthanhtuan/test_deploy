// PaymentController.cs
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WebApp.Helpers; // Đảm bảo bạn có VnPayLibrary ở đây

namespace WebApp.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IConfiguration _configuration;
        public PaymentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult ThanhToanVNPAY(string amount, string orderInfo, string maHopDong, string email)
        {
            string vnp_Returnurl = _configuration["VNPAY:vnp_Returnurl"];
            string vnp_Url = _configuration["VNPAY:vnp_Url"];
            string vnp_TmnCode = _configuration["VNPAY:vnp_TmnCode"];
            string vnp_HashSecret = _configuration["VNPAY:vnp_HashSecret"];

            if (string.IsNullOrEmpty(vnp_TmnCode) || string.IsNullOrEmpty(vnp_HashSecret))
            {
                ViewBag.Message = "Thiếu thông tin cấu hình VNPAY";
                return View("Error");
            }

            string orderId = maHopDong;
            decimal amountDecimal = decimal.Parse(amount, CultureInfo.InvariantCulture);
            int amountInt = (int)(amountDecimal * 100); // x100 theo chuẩn VNPAY

            VnPayLibrary vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", amountInt.ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", orderId);
            vnpay.AddRequestData("vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_OrderInfo", $"{orderInfo} | Email: {email}");
            vnpay.AddRequestData("vnp_SecureHashType", "SHA512");

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return Redirect(paymentUrl);
        }

        [HttpGet]
        public IActionResult KetQuaThanhToan()
        {
            var vnpay = new VnPayLibrary();
            var response = Request.Query;

            foreach (var key in response.Keys)
            {
                vnpay.AddResponseData(key, response[key]);
            }

            string vnp_HashSecret = _configuration["VNPAY:vnp_HashSecret"];
            bool checkSignature = vnpay.ValidateSignature(response["vnp_SecureHash"], vnp_HashSecret);

            if (checkSignature)
            {
                string transactionStatus = response["vnp_TransactionStatus"];
                string maHopDong = response["vnp_TxnRef"];
                string maGiaoDich = response["vnp_TransactionNo"];
                string fullInfo = response["vnp_OrderInfo"];

                string email = "";
                if (!string.IsNullOrEmpty(fullInfo) && fullInfo.Contains("Email:"))
                {
                    email = fullInfo.Split("Email:")[1].Trim();
                }

                string paymentMethod = "VNPAY";

                if (transactionStatus == "00")
                {
                    using (var httpClient = new HttpClient())
                    {
                        string apiUrl = $"https://localhost:7190/api/admin/payment/CapNhatThanhToan?maHopDong={maHopDong}&maGiaoDich={maGiaoDich}&email={email}&phuongThuc={paymentMethod}";
                        var result = httpClient.PutAsync(apiUrl, null).Result;

                        if (result.IsSuccessStatusCode)
                        {
                            return View("KetQuaThanhToanThanhCong");
                        }
                        else
                        {
                            ViewBag.Message = "Thanh toán thành công, nhưng cập nhật hợp đồng thất bại.";
                            return View("Error");
                        }
                    }
                }
                else
                {
                    ViewBag.Message = "Thanh toán không thành công.";
                    return View("Error");
                }
            }
            else
            {
                ViewBag.Message = "Sai chữ ký. Không xác thực được.";
                return View("Error");
            }
        }
    }
}
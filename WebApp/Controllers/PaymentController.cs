// PaymentController.cs
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net.Http;
using WebApp.DTO;
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

        public async Task<IActionResult> Index(string fileName, string email)
        {
            using (var client = new HttpClient())
            {
                var apiUrl = $"https://localhost:7190/api/admin/SeeContract_Sign/CheckStatus?fileName={fileName}&email={email}";

                try
                {
                    var response = await client.GetAsync(apiUrl);
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var hopDong = JsonConvert.DeserializeObject<StatusSignClient>(jsonString);
                    if (!response.IsSuccessStatusCode)
                    {
                        ViewBag.ErrorMessage = "Không tìm thấy hợp đồng hoặc lỗi hệ thống.";
                        return View("Error");
                    }
                    
                    if (hopDong == null)
                    {
                        ViewBag.ErrorMessage = "Dữ liệu hợp đồng không hợp lệ.";
                        return View("Error");
                    }

                    else if (hopDong.status == 5)
                    {
                        //return View("~/Views/SeeContract/Payment.cshtml", hopDong); 
                        ViewBag.ErrorMessage = "Vui lòng chờ Hệ thống xác nhận hoàn tất hợp đồng";
                        return View("Error");
                    }

                    else if (hopDong.status == 6)
                    {
                        //return View("~/Views/SeeContract/Payment.cshtml", hopDong); 
                        ViewBag.ErrorMessage = "Vui lòng chờ Hệ thống xác nhận hoàn tất hợp đồng";
                        return View("Error");
                    }
                    else if (hopDong.status == 4)
                    {
                        var apiUrl2 = $"https://localhost:7190/api/admin/Payment/GetByContractNumber?contractNumber={hopDong.contractnumber}";

                        var response2 = await client.GetAsync(apiUrl2);
                        var jsonString2 = await response2.Content.ReadAsStringAsync();
                        var hopDong2 = JsonConvert.DeserializeObject<CompanyContractDTOs>(jsonString2);
                        if (response2.IsSuccessStatusCode)
                        {
                           // ViewBag.ContractNumber = hopDong2.contractnumber;
                            return View(hopDong2);
                        }
                    }
                    ViewBag.ErrorMessage = "Trạng thái hợp đồng không xác định.";
                    return View("Error");
                }
                catch (Exception ex)
                {
                    return BadRequest("Có lỗi khi gọi API: " + ex.Message);
                }
            }
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
            string paymentId = json["transactionCode"].ToString();

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

        //[HttpGet]
        //public async Task<IActionResult> KetQuaThanhToan()
        //{
        //    var vnpay = new VnPayLibrary();
        //    var response = Request.Query;

        //    foreach (var key in response.Keys)
        //    {
        //        Console.WriteLine($"{key} = {response[key]}");
        //        //vnpay.AddResponseData(key, response[key]);
        //        vnpay.AddResponseData(key, response[key].ToString().Trim());

        //    }

        //    string vnp_HashSecret = _configuration["VNPAY:vnp_HashSecret"];
        //    bool checkSignature = vnpay.ValidateSignature(response["vnp_SecureHash"], vnp_HashSecret);

        //    string transactionStatus = response["vnp_TransactionStatus"];
        //    string responseCode = response["vnp_ResponseCode"];
        //    string id = response["vnp_TxnRef"];
        //    string maGiaoDich = response["vnp_TransactionNo"];
        //    string fullInfo = response["vnp_OrderInfo"];

        //    string email = "";
        //    if (!string.IsNullOrEmpty(fullInfo) && fullInfo.Contains("Email:"))
        //    {
        //        email = fullInfo.Split("Email:")[1].Trim();
        //    }

        //    string paymentMethod = "VNPAY";
        //    string tinhTrang = (transactionStatus == "00" && responseCode == "00") ? "Thanh cong" : "That bai";

        //    if (!checkSignature)
        //    {
        //        tinhTrang = "Sai chữ ký"; // ❗ Cho biết là lỗi chữ ký
        //    }

        //    try
        //    {
        //        using (var httpClient = new HttpClient())
        //        {
        //            var requestBody = new
        //            {
        //                ID = id,
        //                MaGiaoDich = maGiaoDich,
        //                PhuongThuc = paymentMethod,
        //                TinhTrang = tinhTrang
        //            };

        //            var responseApi = await httpClient.PostAsJsonAsync("https://localhost:7190/api/admin/payment/CapNhatThanhToan", requestBody);

        //            if (responseApi.IsSuccessStatusCode)
        //            {
        //                if (checkSignature && tinhTrang == "Thanh cong")
        //                    return View("KetQuaThanhToanThanhCong");
        //                else
        //                {
        //                    ViewBag.Message = "Thanh toán thất bại hoặc sai chữ ký.";
        //                    return View("Thanhtoanthatbai");
        //                }
        //            }
        //            else
        //            {
        //                string err = await responseApi.Content.ReadAsStringAsync();
        //                ViewBag.Message = $"Kết nối thành công nhưng cập nhật thất bại: {err}";
        //                return View("Error");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Message = $"Lỗi hệ thống khi gửi dữ liệu thanh toán: {ex.Message}";
        //        return View("Error");
        //    }
        //}
        [HttpGet]
        public async Task<IActionResult> KetQuaThanhToan()
        {
            var vnpay = new VnPayLibrary();
            var response = Request.Query;

            // Thêm tất cả tham số vào thư viện để kiểm tra hash
            foreach (var key in response.Keys)
            {
                vnpay.AddResponseData(key, response[key].ToString().Trim());
            }

            string vnp_HashSecret = _configuration["VNPAY:vnp_HashSecret"];
            bool checkSignature = vnpay.ValidateSignature(response["vnp_SecureHash"], vnp_HashSecret);

            // Lấy các tham số cần thiết
            string vnp_TxnRef = response["vnp_TxnRef"];
            string vnp_TransactionNo = response["vnp_TransactionNo"];
            string vnp_Amount = response["vnp_Amount"];
            string vnp_BankCode = response["vnp_BankCode"];
            string vnp_BankTranNo = response["vnp_BankTranNo"];
            string vnp_CardType = response["vnp_CardType"];
            string vnp_OrderInfo = response["vnp_OrderInfo"];
            string vnp_PayDate = response["vnp_PayDate"];
            string vnp_ResponseCode = response["vnp_ResponseCode"];
            string vnp_TransactionStatus = response["vnp_TransactionStatus"];
            string vnp_TmnCode = response["vnp_TmnCode"];

            // Xử lý thông tin thêm (ví dụ như lấy email từ OrderInfo)
            string email = "";
            if (!string.IsNullOrEmpty(vnp_OrderInfo) && vnp_OrderInfo.Contains("Email:"))
            {
                email = vnp_OrderInfo.Split("Email:")[1].Trim();
            }

            string paymentMethod = "VNPAY";
            string status = (vnp_TransactionStatus == "00" && vnp_ResponseCode == "00") ? "Thanh cong" : "That bai";

            if (!checkSignature)
            {
                status = "Sai chữ ký";
            }

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var requestBody = new
                    {
                        ID = vnp_TxnRef,
                        MaGiaoDich = vnp_TransactionNo,
                        SoTien = vnp_Amount,
                        MaNganHang = vnp_BankCode,
                        MaGiaoDichNganHang = vnp_BankTranNo,
                        LoaiThe = vnp_CardType,
                        NoiDung = vnp_OrderInfo,
                        NgayThanhToan = vnp_PayDate,
                        MaPhanHoi = vnp_ResponseCode,
                        MaWebsite = vnp_TmnCode,
                        PhuongThuc = paymentMethod,
                        TinhTrang = status,
                        Email = email
                    };

                    var responseApi = await httpClient.PostAsJsonAsync("https://localhost:7190/api/admin/payment/CapNhatThanhToan", requestBody);

                    if (responseApi.IsSuccessStatusCode)
                    {
                        if (checkSignature && status == "Thanh cong")
                            return View("KetQuaThanhToanThanhCong");
                        else
                        {
                            ViewBag.Message = "Thanh toán thất bại hoặc sai chữ ký.";
                            return View("Thanhtoanthatbai");
                        }
                    }
                    else
                    {
                        string err = await responseApi.Content.ReadAsStringAsync();
                        ViewBag.Message = $"Cập nhật thất bại: {err}";
                        return View("Error");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Lỗi hệ thống khi xử lý thanh toán: {ex.Message}";
                return View("Error");
            }
        }


    }
}
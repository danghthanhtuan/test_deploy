namespace WebApi.DTO
{
    public class SignPdfWithHandRequest
    {
        public string base64Data { get; set; }
        public string email { get; set; }
        public string fileName { get; set; }
    }

    public class StatusSignClient
    {
        public string email { get; set; }
        public string fileName { get; set; }
        public int? status { get; set; }
        public string contractnumber { get; set; }
        public decimal amount { get; set; }
    }

    public class ThanhToanRequest
    {
        public string ID { get; set; }
        public string MaGiaoDich { get; set; }
        public string PhuongThuc { get; set; }
        public string TinhTrang { get; set; } // "Thanh cong" hoặc "That bai"
    }
    public class PaymentCreateRequest
    {
        public decimal SoTien { get; set; }
        public string MaHopDong { get; set; }
    }

}

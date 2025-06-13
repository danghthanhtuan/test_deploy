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


    public class PaymentCreateRequest
    {
        public decimal SoTien { get; set; }
        public string MaHopDong { get; set; }
    }


    public class PaymentTr
    {
        public string ID { get; set; }
        public string MaGiaoDich { get; set; }
        public string SoTien { get; set; }
        public string MaNganHang { get; set; }
        public string MaGiaoDichNganHang { get; set; }
        public string LoaiThe { get; set; }
        public string NoiDung { get; set; }
        public string NgayThanhToan { get; set; }
        public string MaPhanHoi { get; set; }
        public string MaWebsite { get; set; }
        public string PhuongThuc { get; set; }
        public string TinhTrang { get; set; }
        public string Email { get; set; }
    }


    public class PaymentTransactionDTO
    {
        public int Id { get; set; }
        public string Contractnumber { get; set; } = null!;
        public decimal Amount { get; set; } 
        public bool Paymentstatus { get; set; }
        public string? TransactionCode { get; set; }
        public string? BankCode { get; set; }
        public string? BankTransactionCode { get; set; }
        public string? CardType { get; set; }
        public string? OrderInfo { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? ResponseCode { get; set; }
        public string? TmnCode { get; set; }
        public string? PaymentMethod { get; set; }
        public int? PaymentResult { get; set; }
        public string? Email { get; set; }
        public string ConfileName { get; set; }
        public string FilePath { get; set; }


    }
}

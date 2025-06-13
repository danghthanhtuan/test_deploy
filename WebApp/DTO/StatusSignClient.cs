namespace WebApp.DTO
{
    public class StatusSignClient
    {
        public string email { get; set; }
        public string fileName { get; set; }
        public int status { get; set; }
        public string contractnumber { get; set; }
        public decimal amount { get; set; }

    }
    public class PaymentCreateRequest
    {
        public decimal SoTien { get; set; }
        public string MaHopDong { get; set; }
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

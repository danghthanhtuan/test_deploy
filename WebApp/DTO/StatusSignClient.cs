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
}

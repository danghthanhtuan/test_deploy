namespace WebApp.DTO
{
    public class ContractDTO
    {
        public string CustomerId { get; set; } = null!;
        public string ContractNumber { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime Startdate { get; set; }
        public DateTime Enddate { get; set; }
        public string? ServiceType { get; set; }
        public int chooseMonth { get; set; }
        public bool Customertype { get; set; }

    }
    public class SignContractRequest
    {
        public string Email { get; set; }
        public string FileName { get; set; }
    }
}

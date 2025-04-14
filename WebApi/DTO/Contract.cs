namespace WebApi.DTO
{
    public class ContractDTO
    {
        public string CustomerId { get; set; } = null!;
        public string ContractNumber { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime Startdate { get; set; }
        public DateTime Enddate { get; set; }
        public string ServiceType { get; set; } = null!;
        public int chooseMonth { get; set; }

    }
}

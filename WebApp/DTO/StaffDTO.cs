namespace WebApp.DTO
{
    public class StaffDTO
    {
        public string Staffid { get; set; } = null!;
        public string Staffname { get; set; } = null!;
        public DateTime? Staffdate { get; set; }
        public bool? Staffgender { get; set; }
        public string? Staffaddress { get; set; }
        public string Staffphone { get; set; } = null!;
        public string Department { get; set; } = null!;
        public string Usernamead { get; set; } = null!;
        public string Passwordad { get; set; } = null!;
    }
}

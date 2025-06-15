namespace WebApp.DTO
{
    public class RegulationsDTO
    {
        public string ServiceGroupid { get; set; } 
        public string GroupName { get; set; }
        public decimal Price { get; set; }

        // Danh sách các dịch vụ (có cả Id và tên)
        public List<ServiceTypeDTO> ServiceTypes { get; set; } = new();
    }

    public class ServiceTypeDTO
    {
        public int Id { get; set; }
        public string ServiceTypeNames { get; set; } = null!;
        public string Descriptionsr { get; set; } = null!;
    }

    public class ServiceTypeDTO1
    {
        public int Id { get; set; }
        public string ServiceTypeNames { get; set; } = null!;
        public string Descriptionsr { get; set; } = null!;
        public string GroupName { get; set; }
        public decimal Price { get; set; }
    }
    public class EndowDTO
    {
        public string Endowid { get; set; } 
        public string ServiceGroupid { get; set; } 

        public double Discount { get; set; }

        public DateTime? Startdate { get; set; }

        public DateTime? Enddate { get; set; }

        public int? Duration { get; set; }

        public string? Descriptionendow { get; set; }
        public string GroupName { get; set; } 

    }

    public class ServiceTypeDTO2
    {
        public string ServiceGroupid { get; set; }
        public string ServiceTypeNames { get; set; }
        public decimal Price { get; set; }

    }
}

namespace WebApp.DTO
{
    public class CompanyAccountDTO
    {
        public string CustomerId { get; set; } = null!;

        public string CompanyName { get; set; } = null!;

        public string TaxCode { get; set; }

        public string CompanyAccount { get; set; } = null!;

        public DateTime? AccountIssuedDate { get; set; }

        public string CPhoneNumber { get; set; } = null!;

        public string CAddress { get; set; } = null!;

        public bool CustomerType { get; set; }
        public string ServiceType { get; set; } 
        public string ContractNumber { get; set; } 
        public string RootAccount { get; set; } = null!;

        public string RootName { get; set; } = null!;

        public string RPhoneNumber { get; set; } = null!;

        public bool IsActive { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public bool Gender { get; set; }
        public DateTime? Startdate { get; set; }

        public DateTime? Enddate { get; set; }
        public decimal Amount { get; set; }
        public string? Original { get; set; }
    }
    public class PagingResult<T>
    {
        public int CurrentPage { get; set; }

        public int PageCount { get; set; }
        public List<T> Results { get; set; }

        public int PageSize { get; set; }

        public int RowCount { get; set; }
    }
    public class GetListCompanyPaging
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string Keyword { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
    public class updateID
    {
        public bool status { get; set; }
        public string CustomerId { get; set; } = null!;
    }

    public class ExportRequestDTO
    {
        public string Keyword { get; set; }
        public string Category { get; set; }
    }
    public class CompanyContractDTOs
    {
        public string CustomerId { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string TaxCode { get; set; }
        public string CompanyAccount { get; set; } = null!;
        public DateTime? AccountIssuedDate { get; set; }
        public string CPhoneNumber { get; set; } = null!;
        public string CAddress { get; set; } = null!;
        public string RootAccount { get; set; } = null!;
        public string RootName { get; set; } = null!;
        public string RPhoneNumber { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public string ContractNumber { get; set; }
        public DateTime? Startdate { get; set; }
        public DateTime? Enddate { get; set; }
        public bool CustomerType { get; set; }
        public string ServiceType { get; set; }
        public string ConfileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string? ChangedBy { get; set; }
        public decimal Amount { get; set; }
        public string? Original { get; set; }
        public int? Constatus { get; set; }

    }

    public class SignAdminRequest
    {
        public string StaffId { get; set; }
        public string FilePath { get; set; }
        public string ContractNumber { get; set; }

    }

}

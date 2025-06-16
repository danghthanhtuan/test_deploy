namespace WebApp.DTO
{
    public class GetListContactPaging
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public int Status { get; set; } // -1 All, 1 Đã liên hệ, 0 chưa liên hệ

        public string KeyWord { get; set; } = string.Empty;
    }

    public class ListContactResponse
    {
        public bool success { get; set; }
        public GetListContactRes data { get; set; }
    }

    public class GetListContactRes
    {
        public List<ContactModelRes> Data { get; set; }
        public int TotalRow { get; set; }
    }

    public class ContactModelRes
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string? Subject { get; set; }

        public string? Message { get; set; }

        public int Status { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}

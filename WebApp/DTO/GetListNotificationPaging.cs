namespace WebApp.DTO
{
    public class GetListNotificationPaging
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int IsRead { get; set; }
        public string KeyWord { get; set; } = string.Empty;
    }

    public class ListNotificationResponse
    {
        public bool success { get; set; }
        public GetListNotificationRes data { get; set; }
    }

    public class GetListNotificationRes
    {
        public int countUnRead { get; set; }
        public List<NotificationModelRes> data { get; set; }
        public bool isNextPage { get; set; } = false;
        public int TotalRow { get; set; }
    }

    public class NotificationModelRes
    {
        public int id { get; set; }

        public int? userId { get; set; }

        public string title { get; set; }

        public string content { get; set; }

        public int typenoti { get; set; }

        public long? referenceId { get; set; }

        public bool isRead { get; set; }

        public string? data { get; set; }

        public DateTime createdAt { get; set; }

        public DateTime? updatedAt { get; set; }
    }
}

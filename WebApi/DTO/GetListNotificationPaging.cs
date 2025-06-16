namespace WebApi.DTO
{
    public class GetListNotificationPaging
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int IsRead { get; set; } // -1 All, 1 Read, 0 UnRead

        public string KeyWord { get; set; } = string.Empty;

        public void CheckValue()
        {
            if (Page <= 0)
                Page = 1;

            if (PageSize <= 0)
                PageSize = 10;
        }
    }

    public class GetListNotificationRes
    {
        public int CountUnRead { get; set; }
        public List<NotificationModelRes> Data { get; set; }
        public bool IsNextPage { get; set; } = false;

        public int TotalRow { get; set; }

    }

    public class NotificationModelRes
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public int Typenoti { get; set; }

        public long? ReferenceId { get; set; }

        public bool IsRead { get; set; }

        public string? Data { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

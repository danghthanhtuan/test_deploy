namespace WebApp.DTO
{
    public class GetListNotificationPaging
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int IsRead { get; set; }
    }
}

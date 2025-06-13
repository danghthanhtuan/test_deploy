namespace WebApi.DTO
{
    public class CreateNotificationDTO
    {
        public int? UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int Type { get; set; }
        public long? ReferenceId { get; set; }
        public string? Data { get; set; }
    }

    public class CreateNotificationResponseDTO
    {
        public int Id { get; set; }
        public string Message { get; set; }
    }
}

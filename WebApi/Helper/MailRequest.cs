namespace WebApi.Helper
{
    public class MailRequest
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class MailRequesta
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        // Thêm cho đính kèm
        public byte[]? AttachmentData { get; set; }
        public string? AttachmentName { get; set; }
    }

}

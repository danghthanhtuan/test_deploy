namespace WebApi.DTO
{
    public class SignPdfWithHandRequest
    {
        public string base64Data { get; set; }
        public string email { get; set; }
        public string fileName { get; set; }
    }

    public class StatusSignClient
    {
        public string email { get; set; }
        public string fileName { get; set; }
        public string status { get; set; }
        public string contractnumber { get; set; }

    }
}

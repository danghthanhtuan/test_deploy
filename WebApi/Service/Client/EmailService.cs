using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using WebApi.Helper;
using WebApi.Service.Client;
using System.Net.Mail;
using System.Net;

namespace WebApi.Service.Client
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;

            // Debug kiểm tra giá trị
            Console.WriteLine($"Email: {_emailSettings.Email}");
            Console.WriteLine($"Display Name: {_emailSettings.Displayname}");
            Console.WriteLine($"Host: {_emailSettings.Host}");
            Console.WriteLine($"Port: {_emailSettings.Port}");
        }

        public Task SendEmailAsync(MailRequest mailRequest)
        {
            throw new NotImplementedException();
        }

        public async Task SendContractEmail(string toEmail, string companyName, string signingLink)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_emailSettings.Email);
            email.From.Add(MailboxAddress.Parse(_emailSettings.Email));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "Yêu cầu ký hợp đồng dịch vụ";

            var builder = new BodyBuilder();
            builder.HtmlBody = $@"
        <p>Chào <strong>{companyName}</strong>,</p>
        <p>Hệ thống đã tạo hợp đồng dịch vụ cho công ty của bạn.</p>
        <p>Vui lòng nhấn vào liên kết sau để xem và ký hợp đồng:</p>
        <p><a href='{signingLink}'>Ký hợp đồng tại đây</a></p>
        <br/>
        <p>Trân trọng,<br/>{_emailSettings.Displayname}</p>";

            email.Body = builder.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

    }
}
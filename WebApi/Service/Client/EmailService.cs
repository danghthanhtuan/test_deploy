using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using WebApi.Helper;
using WebApi.Service.Client;

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


        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSettings.Displayname, _emailSettings.Email));
            email.To.Add(new MailboxAddress("", mailRequest.ToEmail));
            email.Subject = mailRequest.Subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = mailRequest.Body
            };
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
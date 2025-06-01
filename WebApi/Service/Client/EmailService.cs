using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using WebApi.Helper;
using WebApi.Service.Client;
using System.Net.Mail;
using System.Net;
using WebApi.DTO;
using WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Service.Client
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ManagementDbContext _context;
        private readonly IConfiguration _config;

        public EmailService(IOptions<EmailSettings> emailSettings, ManagementDbContext context, IConfiguration config)
        {
            _emailSettings = emailSettings.Value;
            _context = context;
            // Debug kiểm tra giá trị
            Console.WriteLine($"Email: {_emailSettings.Email}");
            Console.WriteLine($"Display Name: {_emailSettings.Displayname}");
            Console.WriteLine($"Host: {_emailSettings.Host}");
            Console.WriteLine($"Port: {_emailSettings.Port}");
            _config = config;
        }

        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress(_emailSettings.Email, _emailSettings.Displayname),
                    Subject = mailRequest.Subject,
                    Body = mailRequest.Body,
                    IsBodyHtml = true
                };
                mail.To.Add(mailRequest.ToEmail);

                using (var smtp = new System.Net.Mail.SmtpClient(_emailSettings.Host, _emailSettings.Port))
                {
                    smtp.Credentials = new NetworkCredential(_emailSettings.Email, _emailSettings.Password);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Gửi email thất bại: {ex.Message}");
                throw;
            }
        }
        
        //gửi hợp đồng cho client
        public async Task SendEmailtoclient(SignAdminRequest dto)
        {
            try
            {
                ////Tạo link xem file PDF
                string fileName = Path.GetFileName(dto.FilePath);
                var result = await (from c in _context.Contracts
                                    join comp in _context.Companies on c.Customerid equals comp.Customerid
                                    join acc in _context.Accounts on comp.Customerid equals acc.Customerid
                                    where c.Contractnumber == dto.ContractNumber
                                    select new
                                    {
                                        Contract = c,
                                        Company = comp,
                                        Account = acc
                                    }).FirstOrDefaultAsync();
                if (result == null)
                    throw new Exception("Không tìm thấy thông tin hợp đồng hoặc công ty.");
                // Tạo link ký hợp đồng
                string razorDomain = _config["App:RazorBaseUrl"] ?? "https://localhost:7176";
                string signingLink = $"{razorDomain}/SeeContract/Index?fileName={fileName}&email={result.Account.Rootaccount}";

                var email = new MimeMessage();
                email.Sender = MailboxAddress.Parse(_emailSettings.Email);
                email.From.Add(MailboxAddress.Parse(_emailSettings.Email));
                email.To.Add(MailboxAddress.Parse(result.Account.Rootaccount));

                email.Subject = "Yêu cầu ký hợp đồng dịch vụ";

                var builder = new BodyBuilder();
                builder.HtmlBody = $@"
        <p>Chào <strong>{result.Company.Companyname}</strong>,</p>
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
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi gửi email: {ex.Message}");
                throw;
            }
        }

        //gửi hợp đồng cho client
        public async Task SendContractEmail(string toEmail, string companyName, string signingLink)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi gửi email: {ex.Message}");
                throw;
            }
        }
        
        public async Task SendAdminNotificationEmail(string adminEmail, string signerEmail, string contractUrl)
        {
            try
            {
                var email = new MimeMessage();
                email.Sender = MailboxAddress.Parse(_emailSettings.Email);
                email.From.Add(MailboxAddress.Parse(_emailSettings.Email));
                email.To.Add(MailboxAddress.Parse(adminEmail));

                email.Subject = "Thông báo hợp đồng đã được ký";

                var builder = new BodyBuilder();
                builder.HtmlBody = $@"
<p><strong>{signerEmail}</strong> đã ký hợp đồng.</p>
<p>Bạn có thể xem hợp đồng tại: <a href='{contractUrl}'>Xem hợp đồng</a></p>
<br/>
<p>Trân trọng,<br/>{_emailSettings.Displayname}</p>";

                email.Body = builder.ToMessageBody();

                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi gửi email thông báo admin: {ex.Message}");
                throw;
            }
        }

        public async Task SendFinalContractToCustomer(string toEmail, string contractUrl)
        {
            try
            {
                var email = new MimeMessage();
                email.Sender = MailboxAddress.Parse(_emailSettings.Email);
                email.From.Add(MailboxAddress.Parse(_emailSettings.Email));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = "Hợp đồng đã hoàn tất";

                var builder = new BodyBuilder();
                builder.HtmlBody = $@"
            <p>Xin chào,</p>
            <p>Hợp đồng của bạn đã được phê duyệt và hoàn tất.</p>
            <p>Bạn có thể xem hoặc tải hợp đồng tại liên kết sau: <a href='{contractUrl}'>Xem hợp đồng</a></p>
            <br/>
            <p>Trân trọng,<br/>{_emailSettings.Displayname}</p>";

                email.Body = builder.ToMessageBody();

                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi gửi hợp đồng final cho khách hàng: {ex.Message}");
                throw;
            }
        }


    }
}
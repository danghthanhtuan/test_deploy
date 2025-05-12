using Microsoft.EntityFrameworkCore;
using WebApi.Content;
using WebApi.DTO;
using WebApi.Helper;
using WebApi.Models;
using WebApi.Service.Client;
//using WebApi.Models;

namespace WebApi.Service.Admin
{
    public class HomeService
    {
        private readonly ManagementDbContext _context;
        private readonly IEmailService _emailService;

        public HomeService(ManagementDbContext context, IEmailService emailService)
        {
            _emailService = emailService;
            _context = context;
        }


        public Staff LoginAsync(string username, string password)
        {
            try
            {
                var nhanVien =
                (from nv in _context.Staff
                 join lg in _context.Loginadmins
                 on nv.Staffid equals lg.Staffid
                 where lg.Usernamead == username
                 select new
                 {
                     nv.Staffid,
                     nv.Staffname,
                     nv.Staffphone,
                     lg.Passwordad, // Lấy mật khẩu đã mã hóa từ DB
                     nv.Department
                 }).FirstOrDefault();

                if (nhanVien != null && BCrypt.Net.BCrypt.Verify(password, nhanVien.Passwordad))
                {
                    return new Staff
                    {
                        Staffid = nhanVien.Staffid,
                        Staffname = nhanVien.Staffname,
                        Staffphone = nhanVien.Staffphone,
                        Department = nhanVien.Department
                    };
                }

                return null; // Đăng nhập thất bại
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public string CheckLogin(string username, string password)
        {
            try
            {
                var user = _context.Loginadmins.FirstOrDefault(x => x.Usernamead == username);

                if (user != null)
                {
                    //string rawPassword = "lien1234";
                    //string hash = BCrypt.Net.BCrypt.HashPassword(rawPassword);
                    //Console.WriteLine(hash);

                    return BCrypt.Net.BCrypt.Verify(password, user.Passwordad)
                        ? "Đăng nhập thành công"
                        : "Mật khẩu không chính xác";
                }
                else
                {
                    return "Tài khoản không tồn tại";
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string> SendEmail_OTP(string phoneNumber, string userEmail)
        {
            try
            {
                // Kiểm tra người dùng
                var user = (from login in _context.Loginadmins
                            join acc in _context.Staff on login.Usernamead equals acc.Staffphone
                            where login.Usernamead == phoneNumber && acc.Staffemail == userEmail
                            select new
                            {
                                login,
                                acc.Staffemail
                            }).FirstOrDefault();

                if (user == null)
                {
                    return "Số điện thoại hoặc email không hợp lệ!";
                }

                // Tạo mã OTP
                Random rd = new Random();
                string otpCode = rd.Next(100000, 1000000).ToString();

                // Xoá OTP cũ (nếu cần), bạn có thể chọn xoá theo Email
                var existingOtp = _context.Passwordresettokens
                    .Where(p => p.Email == userEmail && p.Isused == false)
                    .ToList();
                _context.Passwordresettokens.RemoveRange(existingOtp);

                // Lưu OTP vào bảng PasswordResetToken
                var newToken = new Passwordresettoken
                {
                    Email = userEmail,
                    Otp = otpCode,
                    Expirytime = DateTime.Now.AddMinutes(5),
                    Isused = false
                };

                _context.Passwordresettokens.Add(newToken);
                await _context.SaveChangesAsync();

                // Gửi email
                var emailSender = new SendEmailRegister();
                MailRequest mailRequest = new MailRequest
                {
                    ToEmail = userEmail,
                    Subject = "Mã OTP khôi phục mật khẩu",
                    Body = emailSender.SendEmail_Register(int.Parse(otpCode), userEmail)
                };

                await _emailService.SendEmailAsync(mailRequest);

                return "Ok";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public async Task<string> CheckEmail_Register(string phoneNumber, string otp)
        {
            try
            {
                var record = await (
                    from token in _context.Passwordresettokens
                    join acc in _context.Staff
                        on token.Email equals acc.Staffemail
                    where acc.Staffphone == phoneNumber
                          && token.Otp == otp
                          && token.Isused == false
                    select token
                ).FirstOrDefaultAsync();

                if (record == null)
                {
                    return "Mã OTP không chính xác hoặc không tồn tại!";
                }

                if (record.Expirytime < DateTime.Now)
                {
                    return "Mã OTP đã hết hạn!";
                }

                // Đánh dấu đã dùng
                record.Isused = true;
                await _context.SaveChangesAsync();

                return "OTP hợp lệ!";
            }
            catch (Exception)
            {
                return "Lỗi hệ thống, vui lòng thử lại!";
            }
        }

        public async Task<(bool Success, string Message)> UpdatePassword(LoginRequesta model)
        {
            if (string.IsNullOrEmpty(model.PassWord) || string.IsNullOrEmpty(model.UserName))
            {
                return (false, "Vui lòng nhập đầy đủ thông tin.");
            }

            var existingstaff = await _context.Staff
                .FirstOrDefaultAsync(a => a.Staffphone == model.UserName);

            if (existingstaff == null)
            {
                return (false, "Số điện thoại không hợp lệ! Vui lòng kiểm tra lại.");
            }

            var Staffid = existingstaff.Staffid;

            // Kiểm tra xem số điện thoại đã tồn tại trong bảng LOGINclient chưa
            var existingLogin = await _context.Loginadmins
                .FirstOrDefaultAsync(l => l.Usernamead == model.UserName);

            if (existingLogin == null)
            {
                return (false, "Số điện thoại này chưa đăng ký tài khoản.");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.PassWord);

                    // **Bước 1: Cập nhật mật khẩu mới trong bảng LOGINadmin**
                    existingLogin.Passwordad = hashedPassword;
                    existingLogin.Staffid = Staffid;
                    existingLogin.Usernamead = model.UserName;
                    _context.Loginadmins.Update(existingLogin);

                    // Lưu thay đổi vào database
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "Cập nhật mật khẩu mới thành công!");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    return (false, "Cập nhật mật khẩu thất bại, vui lòng thử lại.");
                }
            }
        }
    }
}

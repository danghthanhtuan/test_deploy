using AutoMapper.Internal;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Text.Json;
using WebApi.Content;
using WebApi.DTO;
using WebApi.Helper;
using WebApi.Models;

namespace WebApi.Service.Client
{
    public class LoginService
    {
        private readonly IEmailService _emailService;

        private readonly ManagementDbContext _context;
        private static int OTP_email;

        public LoginService(ManagementDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        public Accountlogin LoginAsync( string username, string password, string contractnumber)
        {
            try
            {
                var client =
                (from cl in _context.Accounts
                 join lg in _context.Loginclients
                 on cl.Customerid equals lg.Customerid
                 join contract in _context.Contracts
                 on lg.Customerid equals contract.Customerid
                 where lg.Username == username && contract.Contractnumber == contractnumber && contract.IsActive == true
                 select new
                 {
                     cl.Customerid,
                     cl.Rootname,
                     cl.Rphonenumber,
                     lg.Passwordclient, 
                     contract.Contractnumber// Lấy mật khẩu đã mã hóa từ DB
                 }).FirstOrDefault();

                if (client != null && BCrypt.Net.BCrypt.Verify(password, client.Passwordclient))
                {
                    return new Accountlogin
                    {
                        Customerid = client.Customerid,
                        Rootname = client.Rootname,
                        Rphonenumber = client.Rphonenumber, 
                        Contractnumber = client.Contractnumber
                    };
                }

                return null; // Đăng nhập thất bại
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public string CheckLogin(string username, string password, string contractnumber)
        {
            try
            {
                var user = _context.Loginclients.FirstOrDefault(x => x.Username == username);

                if (user != null)
                {
                    return BCrypt.Net.BCrypt.Verify(password, user.Passwordclient)
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

        public async Task<(bool Success, string Message)> UpdatePassword(LoginRequesta model)
        {
            if (string.IsNullOrEmpty(model.PassWord) || string.IsNullOrEmpty(model.UserName))
            {
                return (false, "Vui lòng nhập đầy đủ thông tin.");
            }

            // Kiểm tra xem tài khoản có tồn tại trong bảng Account không
            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Rphonenumber == model.UserName);

            if (existingAccount == null)
            {
                return (false, "Số điện thoại không hợp lệ! Vui lòng kiểm tra lại.");
            }

            var customerId = existingAccount.Customerid;

            // Kiểm tra xem số điện thoại đã tồn tại trong bảng LOGINclient chưa
            var existingLogin = await _context.Loginclients
                .FirstOrDefaultAsync(l => l.Username == model.UserName);

            if (existingLogin == null)
            {
                return (false, "Số điện thoại này chưa đăng ký tài khoản.");
            }

            // Lấy mật khẩu cũ gần nhất từ bảng RESETPASSWORD để kiểm tra trùng mật khẩu
            var lastPasswordEntry = await _context.Resetpasswords
                .Where(rp => rp.Username == model.UserName && rp.Customerid == customerId)
                .OrderByDescending(rp => rp.Id)  // Lấy bản ghi gần nhất theo ID
                .FirstOrDefaultAsync();

            // Kiểm tra nếu mật khẩu mới trùng với mật khẩu gần nhất
            if (lastPasswordEntry != null && BCrypt.Net.BCrypt.Verify(model.PassWord, lastPasswordEntry.Passwordclient))
            {
                return (false, "Mật khẩu mới không được trùng với mật khẩu cũ!");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.PassWord);

                    // **Bước 1: Cập nhật mật khẩu mới trong bảng LOGINclient**
                    existingLogin.Passwordclient = hashedPassword;
                    existingLogin.Customerid = customerId;
                    existingLogin.Username = model.UserName;
                    _context.Loginclients.Update(existingLogin);

                    // **Bước 2: Thêm bản ghi mới vào bảng RESETPASSWORD**
                    var newResetPasswordEntry = new Resetpassword
                    {
                        Customerid = customerId,
                        Username = model.UserName,
                        Passwordclient = hashedPassword
                    };
                    _context.Resetpasswords.Add(newResetPasswordEntry);

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

        public async Task<string> SendEmail_OTP(string phoneNumber, string userEmail)
        {
            try
            {
                // Kiểm tra người dùng
                var user = (from login in _context.Loginclients
                            join acc in _context.Accounts on login.Username equals acc.Rphonenumber
                            where login.Username == phoneNumber && acc.Rootaccount == userEmail
                            select new
                            {
                                login,
                                acc.Rootaccount
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
                    join acc in _context.Accounts
                        on token.Email equals acc.Rootaccount
                    where acc.Rphonenumber == phoneNumber
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



    }
}

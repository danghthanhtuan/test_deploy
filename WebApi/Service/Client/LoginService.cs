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
        private readonly IDistributedCache _cache;
        private static int OTP_email;

        public LoginService(ManagementDbContext context, IEmailService emailService, IDistributedCache cache)
        {
            _context = context;
            _emailService = emailService;
            _cache = cache;
        }
        public Account LoginAsync( string username, string password)
        {
            try
            {
                var client =
                (from cl in _context.Accounts
                 join lg in _context.Loginclients
                 on cl.Customerid equals lg.Customerid
                 where lg.Username == username
                 select new
                 {
                     cl.Customerid,
                     cl.Rootname,
                     cl.Rphonenumber,
                     lg.Passwordclient // Lấy mật khẩu đã mã hóa từ DB
                 }).FirstOrDefault();

                if (client != null && BCrypt.Net.BCrypt.Verify(password, client.Passwordclient))
                {
                    return new Account
                    {
                        Customerid = client.Customerid,
                        Rootname = client.Rootname,
                        Rphonenumber = client.Rphonenumber
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

        public async Task<string> SendEmail_OTP(string phoneNumber, string userEmail)
        {
            try
            {
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

                Random rd = new Random();
                int otpCode = rd.Next(100000, 1000000);
                OTP_email = otpCode;

                // Lưu OTP vào Redis
                await _cache.SetStringAsync(phoneNumber, otpCode.ToString(), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                });

                var emailSender = new SendEmailRegister();
                MailRequest mailRequest = new MailRequest
                {
                    ToEmail = userEmail,
                    Subject = "Mã OTP khôi phục mật khẩu",
                    Body = emailSender.SendEmail_Register(otpCode, userEmail)
                };

                await _emailService.SendEmailAsync(mailRequest);
                return "Ok";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        //public bool CheckEMmail_Register(int otp)
        //{
        //    try
        //    {
        //        if (otp == OTP_email)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }

        //}

        public async Task<string> CheckEMmail_Register(string phoneNumber, int otp)
        {
            try
            {
                // Lấy OTP từ Redis
                var storedOtp = await _cache.GetStringAsync(phoneNumber);

                if (string.IsNullOrEmpty(storedOtp))
                {
                    return "Mã OTP đã hết hạn!";
                }

                if (otp.ToString() == storedOtp)
                {
                    // Xóa OTP sau khi xác thực thành công
                    await _cache.RemoveAsync(phoneNumber);
                    return "OTP hợp lệ!";
                }

                return "Mã OTP không chính xác!";
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



        //public async Task<(bool Success, string Message)> UpdatePassword(LoginRequesta model)
        //{
        //    if (string.IsNullOrEmpty(model.PassWord) || string.IsNullOrEmpty(model.UserName))
        //    {
        //        return (false, "Vui lòng nhập đầy đủ thông tin.");
        //    }

        //    // Kiểm tra xem công ty có tồn tại trong bảng Account không
        //    var existingAccount = await _context.Accounts
        //        .FirstOrDefaultAsync(a => a.RPhoneNumber == model.UserName);

        //    if (existingAccount == null)
        //    {
        //        return (false, "Số điện thoại không hợp lệ! Vui lòng kiểm tra lại.");
        //    }

        //    // Lấy CustomerId từ Account
        //    var customerId = existingAccount.CustomerId;

        //    // Kiểm tra xem số điện thoại đã tồn tại trong bảng LOGINclient chưa
        //    var existingLogin = await _context.Loginclients
        //        .FirstOrDefaultAsync(l => l.UserName == model.UserName);

        //    if (existingLogin == null)
        //    {
        //        return (false, "Số điện thoại này chưa đăng ký tài khoản.");
        //    }

        //    using (var transaction = await _context.Database.BeginTransactionAsync())
        //    {
        //        try
        //        {
        //            // Mã hóa mật khẩu
        //            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.PassWord);

        //            // Cập nhật mật khẩu trong bảng LOGINclient
        //            existingLogin.PassWordclient = hashedPassword;
        //            existingLogin.CustomerId = customerId; // Gán CustomerId tìm được
        //            existingLogin.UserName = model.UserName;


        //            await _context.SaveChangesAsync();
        //            await transaction.CommitAsync();

        //            return (true, "Cập nhật mật khẩu mới thành công!");
        //        }
        //        catch
        //        {
        //            await transaction.RollbackAsync();
        //            return (false, "Cập nhật mật khẩu thất bại, vui lòng thử lại.");
        //        }
        //    }
        //}

    }
}

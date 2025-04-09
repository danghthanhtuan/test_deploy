using Microsoft.EntityFrameworkCore;
using WebApi.DTO;
using WebApi.Models;

namespace WebApi.Service.Client
{
        public class CRegisterService
        {
            private readonly ManagementDbContext _context;
            public CRegisterService(ManagementDbContext context)
            {
                _context = context;
            }
        public async Task<(bool Success, string Message)> RegisterAsync(RegisterclientDTO model)
        {
            if (string.IsNullOrEmpty(model.companyID) ||  string.IsNullOrEmpty(model.rootPhone) || string.IsNullOrEmpty(model.PassWord))
            {
                return (false, "Vui lòng nhập đầy đủ thông tin.");
            }

            // Kiểm tra xem công ty có tồn tại trong bảng Account không
            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Customerid  == model.companyID && a.Rphonenumber == model.rootPhone);

            if (existingAccount == null)
            {
                return (false, "Mã công ty hoặc số điện thoại không hợp lệ! Vui lòng kiểm tra lại.");
            }
            // Kiểm tra xem số điện thoại đã tồn tại trong bảng LOGINclient chưa
            var existingLogin = await _context.Loginclients
                .FirstOrDefaultAsync(l => l.Username == model.rootPhone);

            if (existingLogin != null)
            {
                return (false, "Số điện thoại này đã được đăng ký tài khoản.");
            }
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Mã hóa mật khẩu
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.PassWord);

                    // Thêm vào bảng LOGINclient
                    var loginClient = new Loginclient
                    {
                        Customerid = model.companyID,
                        Username = model.rootPhone,
                        Passwordclient = hashedPassword
                    };

                    _context.Loginclients.Add(loginClient);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return (true, "Đăng ký thành công!");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    return (false, "Đăng ký thất bại, vui lòng thử lại.");
                }
            }
        }
    }

}

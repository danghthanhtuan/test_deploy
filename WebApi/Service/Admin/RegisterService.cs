using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApi.DTO;
using WebApi.Models;

namespace WebApi.Service.Admin
{
    public class RegisterService
    {
        private readonly ManagementDbContext _context;
        public RegisterService(ManagementDbContext context)
        {
            _context = context;
        }
        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDTO model)
        {
            if (string.IsNullOrEmpty(model.StaffName) || string.IsNullOrEmpty(model.PassWordAd) || string.IsNullOrEmpty(model.StaffPhone) || string.IsNullOrEmpty(model.Department))
            {
                return (false, "Vui lòng nhập đầy đủ thông tin.");
            }

            // Kiểm tra số điện thoại đã tồn tại chưa
            if (await _context.Staff.AnyAsync(u => u.Staffphone == model.StaffPhone))
            {
                return (false, "Số điện thoại đăng ký đã tồn tại.");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Tìm ID lớn nhất hiện có trong bảng STAFF
                    int maxId = await _context.Staff.MaxAsync(s => (int?)s.Id) ?? 0;
                    int newId = maxId + 1;
                    string newStaffId = "Sta" + newId; // Tạo staffID

                    // Mã hóa mật khẩu
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.PassWordAd);

                    // Tạo đối tượng nhân viên mới với staffID
                    var user = new Staff
                    {
                        Staffid = newStaffId, // Gán staffID trước khi insert
                        Staffname = model.StaffName,
                        Staffphone = model.StaffPhone,
                        Department = model.Department,
                    };

                    _context.Staff.Add(user);
                    await _context.SaveChangesAsync();

                    // Tạo tài khoản đăng nhập
                    var login = new Loginadmin
                    {
                        Staffid = newStaffId,
                        Usernamead = model.StaffPhone,
                        Passwordad = hashedPassword
                    };

                    _context.Loginadmins.Add(login);
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

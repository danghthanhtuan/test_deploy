using WebApi.Content;
using WebApi.DTO;
using WebApi.Helper;
using WebApi.Models;
using WebApi.Service.Client;

namespace WebApi.Service.Admin
{
    public class StaffService
    {
        private readonly ManagementDbContext _context;
        private readonly IEmailService _emailService;

        public StaffService(ManagementDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public List<Staff> GetAllNhanVien()
        {
            var listNhanVien =
                (from NhanVien in _context.Staff
                 select new Staff
                 {
                     Staffid = NhanVien.Staffid,
                     Staffname = NhanVien.Staffname,
                     Staffemail = NhanVien.Staffemail,
                     Staffgender = NhanVien.Staffgender,
                     Staffaddress = NhanVien.Staffaddress,
                     Staffdate = NhanVien.Staffdate,
                     Staffphone = NhanVien.Staffphone,
                     Department = NhanVien.Department,
                 }
                 ).ToList();
            return listNhanVien;
        }
        public StaffDTO GetById(string id)
        {
            try
            {
                var Staff = (
                    from NhanVien in _context.Staff
                    join LOGIN_NV in _context.Loginadmins
                    on NhanVien.Staffid equals LOGIN_NV.Staffid
                    where NhanVien.Staffid == id
                    select new StaffDTO
                    {
                        Staffid = NhanVien.Staffid,
                        Staffname = NhanVien.Staffname,
                        Staffemail = NhanVien.Staffemail,
                        Staffdate = NhanVien.Staffdate,
                        Staffgender = NhanVien.Staffgender,
                        Staffaddress = NhanVien.Staffaddress,
                        Department = NhanVien.Department,
                        Staffphone = NhanVien.Staffphone,
                        //Usernamead = LOGIN_NV.Usernamead,
                        //Passwordad = LOGIN_NV.Passwordad,
                    }).FirstOrDefault();

                return Staff;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetById: {ex.Message}");
                throw;
            }
        }
        public async Task<bool> ThemNhanVien(StaffDTO obj)
        {
            try
            {
                var existingSDT = _context. Staff.FirstOrDefault(nv => nv.Staffphone == obj.Staffphone);

                if (existingSDT != null)
                {
                    return false;
                }
                else
                {
                    // Tìm mã lớn nhất có prefix "Sta"
                    var lastStaff = _context.Staff
                        .Where(s => s.Staffid.StartsWith("Sta"))
                        .OrderByDescending(s => s.Staffid)
                        .FirstOrDefault();

                    int nextNumber = lastStaff != null
                        ? int.Parse(lastStaff.Staffid.Substring(3)) + 1
                        : 1;

                    string newStaffId = $"Sta{nextNumber}";

                    // Tạo một đối tượng nhan viên mới
                    var newNhanVien = new Staff
                    {
                        Staffid = newStaffId,
                        Staffname = obj.Staffname,
                        Staffemail = obj.Staffemail,
                        Staffgender = obj.Staffgender,
                        Staffaddress = obj.Staffaddress,
                        Staffdate = obj.Staffdate,
                        Staffphone = obj.Staffphone,
                        Department = obj.Department,
                    };

                    _context.Staff.Add(newNhanVien);
                    await _context.SaveChangesAsync();
                    
                    // Tạo mật khẩu ngẫu nhiên
                    string generatedPassword = GenerateRandomPassword();
                    string hashedPassword = HashPassword(generatedPassword);

                    //Tạo một đối tượng loginNV mới
                    var newLoginNV = new Loginadmin
                    {
                        Staffid = newNhanVien.Staffid,
                        Usernamead = obj.Staffphone,
                        Passwordad = hashedPassword
                    };

                    _context.Loginadmins.Add(newLoginNV);
                    await _context.SaveChangesAsync();

                    // Gửi email với mật khẩu
                    await SendPasswordEmail(obj.Staffemail, generatedPassword);
                    return true;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong ThemNhanVien: {ex.Message}");
                throw;
            }
        }
        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@#$%^&*!";
            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        
        private async Task SendPasswordEmail(string email, string password)
        {
            var emailSender = new SendEmailRegister();
            MailRequest mailRequest = new MailRequest
            {
                ToEmail = email,
                Subject = "Tài khoản đăng nhập hệ thống",
                Body = emailSender.SendEmail_pass(password, email)
            };
            await _emailService.SendEmailAsync(mailRequest);
        }

        public bool UpdateThongTinNhanVien(StaffDTO obj)
        {
            try
            {
                // Kiểm tra sdt và userName có trùng với nhân viên nào khác hay không
                var existingSDT = _context.Staff.FirstOrDefault(nv => nv.Staffphone == obj.Staffphone && nv.Staffid != obj.Staffid);

                if (existingSDT != null)
                {
                    //throw new Exception("Số điện thoại đã tồn tại.");
                    return false;
                }
                else
                {
                    var nhanVien = _context.Staff.FirstOrDefault(t => t.Staffid == obj.Staffid);
                    nhanVien.Staffname = obj.Staffname;
                    nhanVien.Staffemail = obj.Staffemail;
                    nhanVien.Staffphone = obj.Staffphone;
                    nhanVien.Staffdate = obj.Staffdate;
                    nhanVien.Staffgender = obj.Staffgender;
                    nhanVien.Staffaddress = obj.Staffaddress;
                    nhanVien.Department = obj.Department;
                    _context.SaveChanges();
                    return true;
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Update: {ex.Message}");
                throw;
            }
        }
    }
}

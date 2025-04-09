using Microsoft.EntityFrameworkCore;
using WebApi.Models;
//using WebApi.Models;

namespace WebApi.Service.Admin
{
    public class HomeService
    {
        private readonly ManagementDbContext _context;
        public HomeService(ManagementDbContext context)
        {
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
    }
}

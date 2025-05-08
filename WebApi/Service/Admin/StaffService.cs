using WebApi.DTO;
using WebApi.Models;

namespace WebApi.Service.Admin
{
    public class StaffService
    {
        private readonly ManagementDbContext _context;
        public StaffService(ManagementDbContext context)
        {
            _context = context;
        }

        public List<Staff> GetAllNhanVien()
        {
            var listNhanVien =
                (from NhanVien in _context.Staff
                 select new Staff
                 {
                     Staffid = NhanVien.Staffid,
                     Staffname = NhanVien.Staffname,
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
                        Staffdate = NhanVien.Staffdate,
                        Staffgender = NhanVien.Staffgender,
                        Staffaddress = NhanVien.Staffaddress,
                        Department = NhanVien.Department,
                        Staffphone = NhanVien.Staffphone,
                        Usernamead = LOGIN_NV.Usernamead,
                        Passwordad = LOGIN_NV.Passwordad,
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
                        Staffgender = obj.Staffgender,
                        Staffaddress = obj.Staffaddress,
                        Staffdate = obj.Staffdate,
                        Staffphone = obj.Staffphone,
                        Department = obj.Department,
                    };

                    _context.Staff.Add(newNhanVien);
                    await _context.SaveChangesAsync();

                    //Tạo một đối tượng loginNV mới
                    var newLoginNV = new Loginadmin
                    {
                        Staffid = newNhanVien.Staffid,
                        Usernamead = obj.Usernamead,
                        //Passwordad = obj.Passwordad,
                        Passwordad = BCrypt.Net.BCrypt.HashPassword(obj.Passwordad), // Mã hóa mật khẩu

                    };

                    _context.Loginadmins.Add(newLoginNV);
                    await _context.SaveChangesAsync();

                    return true;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong ThemNhanVien: {ex.Message}");
                throw;
            }
        }

        public bool UpdateThongTinNhanVien(StaffDTO obj)
        {
            try
            {
                // Kiểm tra sdt và userName có trùng với nhân viên nào khác hay không
                var existingSDT = _context.Staff.FirstOrDefault(nv => nv.Staffphone == obj.Staffphone && nv.Staffid != obj.Staffid);
                //var existingUsername = _context.Loginadmins.FirstOrDefault(login => login.Usernamead == obj.Usernamead && login.Staffid != obj.Staffid);

                if (existingSDT != null)
                {
                    //throw new Exception("Số điện thoại đã tồn tại.");
                    return false;
                }
                else
                {
                    var nhanVien = _context.Staff.FirstOrDefault(t => t.Staffid == obj.Staffid);
                    nhanVien.Staffname = obj.Staffname;
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

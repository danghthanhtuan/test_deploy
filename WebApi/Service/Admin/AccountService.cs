using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Data;
using WebApi.DTO;
using WebApi.Models;
using System.Text;
using WebApi.Content;
using WebApi.Helper;
using WebApi.Service.Client;
using StackExchange.Redis;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebApi.Service.Admin
{

    public class AccountService
    {
        private readonly ManagementDbContext _context;
        private readonly IEmailService _emailService;
        //private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<AccountService> _logger;

        public AccountService(ManagementDbContext context, IEmailService emailService, ILogger<AccountService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<PagingResult<CompanyAccountDTO>> GetAllCompany(GetListCompanyPaging req)
        {
          
            var query = from c in _context.Companies
                        join a in _context.Accounts on c.Customerid equals a.Customerid
                        join b in _context.Contracts on c.Customerid equals b.Customerid
                        join h in _context.ServiceTypes on b.ServiceTypeid equals h.Id
                        group new {c,a,h} by new
                        {
                            c.Customerid,
                            c.Companyname, 
                            c.Taxcode,
                            c.Companyaccount,
                            c.Accountissueddate, 
                            c.Cphonenumber, 
                            c.Caddress,
                            //b.Customertype,
                            //h.ServiceTypename,
                            a.Rootaccount,
                            a.Rootname,
                            a.Rphonenumber,
                            c.Operatingstatus,
                            a.Dateofbirth,
                            a.Gender,
                        } into g
                        select new CompanyAccountDTO
                        {
                            CustomerId = g.Key.Customerid,
                            CompanyName = g.Key.Companyname,
                            TaxCode = g.Key.Taxcode,
                            CompanyAccount = g.Key.Companyaccount,
                            AccountIssuedDate = g.Key.Accountissueddate,
                            CPhoneNumber = g.Key.Cphonenumber,
                            CAddress = g.Key.Caddress,
                            //CustomerType = g.Key.Customertype,
                            //ServiceType = g.Key.ServiceTypename,
                            //ContractNumber = b.Contractnumber,
                            RootAccount = g.Key.Rootaccount,
                            RootName = g.Key.Rootname,
                            RPhoneNumber = g.Key.Rphonenumber,
                            OperatingStatus = g.Key.Operatingstatus,
                            DateOfBirth = g.Key.Dateofbirth,
                            Gender = g.Key.Gender,
                        };


            // Tìm kiếm
            if (!string.IsNullOrEmpty(req.Keyword))
            {
                query = query.Where(c =>
                    c.CompanyName.Contains(req.Keyword) ||
                    c.CustomerId.Contains(req.Keyword) ||
                    c.CompanyAccount.Contains(req.Keyword) ||
                    c.TaxCode.Contains(req.Keyword));
            }

            // Lọc loại khách hàng
            if (req.Category == "Bình thường")
            {
                query = query.Where(c => !c.CustomerType);
            }
            else if (req.Category == "Vip")
            {
                query = query.Where(c => c.CustomerType);
            }

            // Phân trang
            var totalRow = await query.CountAsync();
            var pagedResult = await query
                .OrderByDescending(c => c.CustomerId)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

            var pageCount = (int)Math.Ceiling(totalRow / (double)req.PageSize);

            return new PagingResult<CompanyAccountDTO>
            {
                Results = pagedResult,
                CurrentPage = req.Page,
                RowCount = totalRow,
                PageSize = req.PageSize,
                PageCount = pageCount
            };
        }


        public bool UpdateStatus(bool Tinhtrang, string CustomerID)
        {
            try
            {
                if (string.IsNullOrEmpty(CustomerID))
                {
                    Console.WriteLine("Lỗi: CustomerID bị null hoặc rỗng.");
                    return false;
                }

                var sql = $"UPDATE COMPANY SET OperatingStatus = @p0 WHERE CustomerId = @p1";

                int rowsAffected = _context.Database.ExecuteSqlInterpolated(
                    $"UPDATE COMPANY SET OperatingStatus = {Tinhtrang} WHERE CustomerId = {CustomerID}");

                if (rowsAffected > 0)
                {
                    Console.WriteLine($"Cập nhật thành công CustomerID: {CustomerID}, Status: {Tinhtrang}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Không có bản ghi nào bị ảnh hưởng! CustomerID: {CustomerID}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi SQL: " + ex.Message);
                return false;
            }
        }
        public string? Update(CompanyAccountDTO CompanyAccountDTO, string id)
        {
            if (CompanyAccountDTO == null)
            {
                return "Dữ liệu không hợp lệ.";
            }
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var staff = _context.Staff.FirstOrDefault(s => s.Staffid == id);
                    if (staff == null)
                    {
                        throw new Exception($"Nhân viên với mã nhân viên = {id} không tồn tại");
                    }

                    var existingCompany = _context.Companies.FirstOrDefault(c => c.Customerid == CompanyAccountDTO.CustomerId);
                    if (existingCompany == null)
                    {
                        return "Công ty với mã khách hàng  không tồn tại";
                    }

                    if (_context.Companies.Any(s => s.Cphonenumber == CompanyAccountDTO.CPhoneNumber && s.Customerid != CompanyAccountDTO.CustomerId) ||
    _context.Accounts.Any(a => a.Rphonenumber == CompanyAccountDTO.CPhoneNumber && a.Customerid != CompanyAccountDTO.CustomerId))
                    {
                        return "Số điện thoại đã tồn tại trong hệ thống. Vui lòng kiểm tra lại.";
                    }

                    if (_context.Companies.Any(s => s.Companyaccount == CompanyAccountDTO.CompanyAccount && s.Customerid != CompanyAccountDTO.CustomerId) ||
                        _context.Accounts.Any(a => a.Rootaccount == CompanyAccountDTO.CompanyAccount && a.Customerid != CompanyAccountDTO.CustomerId))
                    {
                        return "Email đã tồn tại trong hệ thống. Vui lòng kiểm tra lại.";
                    }

                    if (_context.Companies.Any(s => s.Taxcode == CompanyAccountDTO.TaxCode && s.Customerid != CompanyAccountDTO.CustomerId))
                    {
                        return "Mã số thuế đã tồn tại trong hệ thống! Vui lòng kiểm tra lại.";
                    }

                    if (_context.Contracts.Any(s => s.Contractnumber == CompanyAccountDTO.ContractNumber && s.Customerid != CompanyAccountDTO.CustomerId ))
                    {
                        return "Số hợp đồng đã tồn tại. Vui lòng kiểm tra lại.";
                    }

                    // Cập nhật thông tin công ty
                    existingCompany.Companyname = CompanyAccountDTO.CompanyName;
                    existingCompany.Taxcode = CompanyAccountDTO.TaxCode;
                    existingCompany.Companyaccount = CompanyAccountDTO.CompanyAccount;
                    //existingCompany.Accountissueddate = CompanyAccountDTO.AccountIssuedDate;
                    existingCompany.Cphonenumber = CompanyAccountDTO.CPhoneNumber;
                    existingCompany.Caddress = CompanyAccountDTO.CAddress;
                    //existingCompany.Customertype = CompanyAccountDTO.CustomerType;
                    //existingCompany.Operatingstatus = CompanyAccountDTO.OperatingStatus;

                    //existingCompany.ServiceType = CompanyAccountDTO.ServiceType;
                    // existingCompany.ContractNumber = CompanyAccountDTO.ContractNumber;
                    _context.Companies.Update(existingCompany);
                    _context.SaveChanges();

                    var existingAccount = _context.Accounts.FirstOrDefault(a => a.Customerid == CompanyAccountDTO.CustomerId);
                    if (existingAccount == null)
                    {
                        return "Tài khoản khách hàng với mã khách hàng không tồn tại";
                    }

                    // Kiểm tra lại số điện thoại root
                    if (_context.Companies.Any(s => s.Cphonenumber == CompanyAccountDTO.RPhoneNumber && s.Customerid != CompanyAccountDTO.CustomerId) ||
                        _context.Accounts.Any(a => a.Rphonenumber == CompanyAccountDTO.RPhoneNumber && a.Customerid != CompanyAccountDTO.CustomerId))
                    {
                        return "Số điện thoại đã tồn tại trong hệ thống. Vui lòng kiểm tra lại.";
                    }

                    // Kiểm tra lại email root
                    if (_context.Companies.Any(s => s.Companyaccount == CompanyAccountDTO.RootAccount && s.Customerid != CompanyAccountDTO.CustomerId) ||
                        _context.Accounts.Any(a => a.Rootaccount == CompanyAccountDTO.RootAccount && a.Customerid != CompanyAccountDTO.CustomerId))
                    {
                        return "Email đã tồn tại trong hệ thống. Vui lòng kiểm tra lại.";
                    }

                    // Lưu số điện thoại root cũ để cập nhật bảng LoginClient
                    var oldRPhoneNumber = existingAccount.Rphonenumber;

                    // Cập nhật thông tin tài khoản
                    existingAccount.Rootaccount = CompanyAccountDTO.RootAccount;
                    existingAccount.Rootname = CompanyAccountDTO.RootName;
                    existingAccount.Rphonenumber = CompanyAccountDTO.RPhoneNumber;
                    //existingAccount.OperatingStatus = CompanyAccountDTO.OperatingStatus;
                    existingAccount.Dateofbirth = (DateTime)CompanyAccountDTO.DateOfBirth!;
                    existingAccount.Gender = CompanyAccountDTO.Gender;

                    _context.Accounts.Update(existingAccount);
                    _context.SaveChanges();

                    // Cập nhật bảng LoginClient nếu số điện thoại đã thay đổi
                    if (oldRPhoneNumber != CompanyAccountDTO.RPhoneNumber)
                    {
                        var loginClient = _context.Loginclients.FirstOrDefault(l => l.Username == oldRPhoneNumber);
                        if (loginClient != null)
                        {
                            loginClient.Username = CompanyAccountDTO.RPhoneNumber;
                            _context.Loginclients.Update(loginClient);
                            _context.SaveChanges();
                        }
                    }

                    var existingContracts = _context.Contracts.FirstOrDefault(a =>
                        a.Customerid == CompanyAccountDTO.CustomerId &&
                        a.Contractnumber == CompanyAccountDTO.ContractNumber);
                    if (existingAccount == null)
                    {
                        return "Tài khoản khách hàng với mã khách hàng không tồn tại";
                    }
                  //  var serviceType = _context.ServiceTypes
    // .FirstOrDefault(st => st.ServiceTypename == CompanyAccountDTO.ServiceType);
    //
                   // if (serviceType == null)
                   // {
                    //    return $"Loại dịch vụ '{CompanyAccountDTO.ServiceType}' không tồn tại.";
                   // }

                    // Gán thông tin mới cho hợp đồng
                   // existingContracts.ServiceTypeid = serviceType.Id; 
                    //existingContracts.Startdate = CompanyAccountDTO.Startdate;
                   // existingContracts.Enddate = CompanyAccountDTO.Enddate;

                   // _context.Contracts.Update(existingContracts);
                    _context.SaveChanges();

                    transaction.Commit();
                    return existingCompany.Customerid;
                }
                catch (DbUpdateException dbEx)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Lỗi SQL: {dbEx.InnerException?.Message}");
                    return "Lỗi SQL, vui lòng kiểm tra dữ liệu đầu vào.";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Lỗi hệ thống: {ex.Message}");
                    return "Lỗi hệ thống, vui lòng thử lại sau.";
                }
            }
        }
        public async Task<byte[]> ExportToCsv(ExportRequestDTO req)
        {
            var query = from c in _context.Companies
                        join a in _context.Accounts on c.Customerid equals a.Customerid
                        group new { c, a } by new
                        {
                            c.Customerid,
                            c.Companyname,
                            c.Cphonenumber,
                            c.Taxcode,
                            c.Companyaccount,
                            c.Accountissueddate,
                            c.Operatingstatus,
                            a.Rootname,
                            a.Rootaccount,
                            a.Rphonenumber,
                            a.Gender,
                            a.Dateofbirth,
                        } into g
                        select new CompanyAccountDTO
                        {
                            CustomerId = g.Key.Customerid,
                            CompanyName = g.Key.Companyname,
                            CPhoneNumber = g.Key.Cphonenumber,
                            TaxCode = g.Key.Taxcode,
                            CompanyAccount = g.Key.Companyaccount,
                            AccountIssuedDate = g.Key.Accountissueddate,
                            OperatingStatus = g.Key.Operatingstatus,
                            RootName = g.Key.Rootname,
                            RootAccount = g.Key.Rootaccount,
                            RPhoneNumber = g.Key.Rphonenumber,
                            Gender = g.Key.Gender,
                            DateOfBirth = g.Key.Dateofbirth,
                        };

            // Lọc theo từ khóa
            if (!string.IsNullOrEmpty(req.Keyword))
            {
                query = query.Where(c =>
                    c.CompanyName.Contains(req.Keyword) ||
                    c.CustomerId.Contains(req.Keyword) ||
                    c.CompanyAccount.Contains(req.Keyword) ||
                    c.TaxCode.Contains(req.Keyword));
            }

            var data = await query.ToListAsync();
            if (!data.Any())
            {
                throw new Exception("Không có dữ liệu để xuất!");
            }

            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
            {
                writer.WriteLine("STT,Mã khách hàng,Tên công ty,Mã số thuế,SDT Công ty,Email công ty,Ngày cấp tài khoản,Trạng thái,Tên KH,Tài khoản KH,Số điện thoại,Giới tính,Ngày sinh");

                int stt = 1;
                foreach (var item in data)
                {
                    string ngayCap = item.AccountIssuedDate?.ToString("dd/MM/yyyy") ?? "";
                    string ngaySinh = item.DateOfBirth?.ToString("dd/MM/yyyy") ?? "";

                    string trangThai = item.OperatingStatus ? "Hoạt động" : "Không hoạt động";

                    string gioiTinh = item.Gender ? "Nam" : "Nữ";
                    // Thêm dấu ' trước số để giữ 0 đầu trong Excel
                    string sdtCongTy = $"'{item.CPhoneNumber}";
                    string sdtKhachHang = $"'{item.RPhoneNumber}";

                    writer.WriteLine($"{stt},{item.CustomerId},{item.CompanyName},{item.TaxCode},{sdtCongTy},{item.CompanyAccount},{ngayCap},{trangThai},{item.RootName},{item.RootAccount},{sdtKhachHang},{gioiTinh},{ngaySinh}");
                    stt++;
                }

                writer.Flush();
                return memoryStream.ToArray();
            }
        }


        public async Task<string?> Insert(CompanyAccountDTO CompanyAccountDTO, string id)
        {
            if (CompanyAccountDTO == null)
            {
                return "Dữ liệu không hợp lệ.";
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var staff = _context.Staff.FirstOrDefault(s => s.Staffid == id);
                    if (staff == null)
                    {
                        return $"Nhân viên với mã nhân viên = {id} không tồn tại";
                    }

                    if (_context.Companies.Any(s => s.Cphonenumber == CompanyAccountDTO.CPhoneNumber) ||
                        _context.Accounts.Any(a => a.Rphonenumber == CompanyAccountDTO.CPhoneNumber))
                    {
                        return "Số điện thoại đã tồn tại trong hệ thống. Vui lòng kiểm tra lại.";
                    }

                    if (_context.Companies.Any(s => s.Companyaccount == CompanyAccountDTO.CompanyAccount) ||
                        _context.Accounts.Any(a => a.Rootaccount == CompanyAccountDTO.CompanyAccount))
                    {
                        return "Email đã tồn tại trong hệ thống. Vui lòng kiểm tra lại.";
                    }

                    if (_context.Companies.Any(s => s.Taxcode == CompanyAccountDTO.TaxCode))
                    {
                        return "Mã số thuế đã tồn tại trong hệ thống! Vui lòng kiểm tra lại.";
                    }

                    var lastCustomer = _context.Companies
                        .Where(c => c.Customerid.StartsWith("IT030300"))
                        .OrderByDescending(c => c.Customerid)
                        .FirstOrDefault();

                    int nextNumber = lastCustomer != null ? int.Parse(lastCustomer.Customerid.Substring(8)) + 1 : 1;
                    string newCustomerID = $"IT030300{nextNumber:D2}";

                    var newCompany = new Company
                    {
                        Customerid = newCustomerID,
                        Companyname = CompanyAccountDTO.CompanyName,
                        Taxcode = CompanyAccountDTO.TaxCode,
                        Companyaccount = CompanyAccountDTO.CompanyAccount,
                        Accountissueddate = CompanyAccountDTO.AccountIssuedDate,
                        Cphonenumber = CompanyAccountDTO.CPhoneNumber,
                        Caddress = CompanyAccountDTO.CAddress,
                        //Customertype = CompanyAccountDTO.CustomerType,
                        Operatingstatus = CompanyAccountDTO.OperatingStatus,

                        // ServiceType = CompanyAccountDTO.ServiceType,
                        // ContractNumber = CompanyAccountDTO.ContractNumber,
                    };

                    _context.Companies.Add(newCompany);
                    _context.SaveChanges();

                    // Tạo mật khẩu ngẫu nhiên
                    string generatedPassword = GenerateRandomPassword();
                    string hashedPassword = HashPassword(generatedPassword);

                    var newAccount = new Account
                    {
                        Customerid = newCustomerID,
                        Rootaccount = CompanyAccountDTO.RootAccount,
                        Rootname = CompanyAccountDTO.RootName,
                        Rphonenumber = CompanyAccountDTO.RPhoneNumber,
                        //OperatingStatus = CompanyAccountDTO.OperatingStatus,
                        Dateofbirth = (DateTime)CompanyAccountDTO.DateOfBirth!,
                        Gender = CompanyAccountDTO.Gender,
                    };
                    _context.Accounts.Add(newAccount);
                    _context.SaveChanges();

                    var newLogin = new Loginclient
                    {
                        Customerid = newCustomerID,
                        Username = CompanyAccountDTO.RPhoneNumber,
                        Passwordclient = hashedPassword
                    };
                    _context.Loginclients.Add(newLogin);
                    _context.SaveChanges();

                    var lastContract = _context.Contracts
                .OrderByDescending(c => c.Contractnumber)
                .FirstOrDefault();
                    if (!_context.ServiceTypes.Any(s => s.ServiceTypename == CompanyAccountDTO.ServiceType))
                    {
                        return "Loại dịch vụ không tồn tại trong hệ thống. Vui lòng kiểm tra lại.";
                    }

                    int nextContractNumber = lastContract != null ? int.Parse(lastContract.Contractnumber.Substring(2)) + 1 : 1;
                    string newContractNumber = $"SV{nextContractNumber:D4}";
                    
                    var serviceType = _context.ServiceTypes
    .FirstOrDefault(st => st.ServiceTypename == CompanyAccountDTO.ServiceType);

                    if (serviceType == null)
                    {
                        return $"Loại dịch vụ '{CompanyAccountDTO.ServiceType}' không tồn tại.";
                    }

                    var newContract = new Contract
                    {
                        Contractnumber = newContractNumber,
                        Startdate = (DateTime)CompanyAccountDTO.Startdate!,
                        Enddate = (DateTime)CompanyAccountDTO.Enddate!,
                        ServiceTypeid = serviceType.Id,
                        Customerid = newCustomerID,
                        Customertype = CompanyAccountDTO.CustomerType,
                    };
                    var newPayment = new Payment
                    {
                        //Customerid = newCustomerID,
                        Contractnumber = newContractNumber,
                        Amount = CompanyAccountDTO.Amount,
                        Paymentstatus = false,
                    };
                    _context.Payments.Add(newPayment);
                    _context.Contracts.Add(newContract);
                    _context.SaveChanges();

                    transaction.Commit();

                    // Gửi email với mật khẩu
                    await SendPasswordEmail(CompanyAccountDTO.RootAccount, generatedPassword);

                    return newCustomerID;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Lỗi hệ thống: {ex.Message}");
                    return "Lỗi hệ thống, vui lòng thử lại sau.";
                }
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


        //public async Task<string?> Insert(CompanyAccountDTO CompanyAccountDTO, string id)
        //{
        //    if (CompanyAccountDTO == null || string.IsNullOrWhiteSpace(id))
        //    {
        //        return "Dữ liệu không hợp lệ.";
        //    }

        //    await using var transaction = await _context.Database.BeginTransactionAsync();

        //    try
        //    {
        //        var staff = await _context.Staff.FindAsync(id);
        //        if (staff == null)
        //        {
        //            return $"Nhân viên với mã nhân viên = {id} không tồn tại.";
        //        }

        //        // Kiểm tra tất cả dữ liệu trùng lặp trong 1 lần truy vấn để tối ưu hiệu suất
        //        var existingData = await _context.Companies
        //            .Where(c => c.CPhoneNumber == CompanyAccountDTO.CPhoneNumber
        //                     || c.CompanyAccount == CompanyAccountDTO.CompanyAccount
        //                     || c.TaxCode == CompanyAccountDTO.TaxCode
        //                     || c.ContractNumber == CompanyAccountDTO.ContractNumber)
        //            .Select(c => new { c.CPhoneNumber, c.CompanyAccount, c.TaxCode, c.ContractNumber })
        //            .ToListAsync();

        //        var existingAccounts = await _context.Accounts
        //            .Where(a => a.RPhoneNumber == CompanyAccountDTO.CPhoneNumber
        //                     || a.RootAccount == CompanyAccountDTO.CompanyAccount)
        //            .Select(a => new { a.RPhoneNumber, a.RootAccount })
        //            .ToListAsync();

        //        if (existingData.Any(c => c.CPhoneNumber == CompanyAccountDTO.CPhoneNumber) ||
        //            existingAccounts.Any(a => a.RPhoneNumber == CompanyAccountDTO.CPhoneNumber))
        //        {
        //            return "Số điện thoại đã tồn tại trong hệ thống. Vui lòng kiểm tra lại.";
        //        }

        //        if (existingData.Any(c => c.CompanyAccount == CompanyAccountDTO.CompanyAccount) ||
        //            existingAccounts.Any(a => a.RootAccount == CompanyAccountDTO.CompanyAccount))
        //        {
        //            return "Email đã tồn tại trong hệ thống. Vui lòng kiểm tra lại.";
        //        }

        //        if (existingData.Any(c => c.TaxCode == CompanyAccountDTO.TaxCode))
        //        {
        //            return "Mã số thuế đã tồn tại trong hệ thống! Vui lòng kiểm tra lại.";
        //        }

        //        if (existingData.Any(c => c.ContractNumber == CompanyAccountDTO.ContractNumber))
        //        {
        //            return "Số hợp đồng đã tồn tại. Vui lòng kiểm tra lại.";
        //        }

        //        // Sinh mã khách hàng mới
        //        var lastCustomer = await _context.Companies
        //            .Where(c => c.CustomerId.StartsWith("IT030300"))
        //            .OrderByDescending(c => c.CustomerId)
        //            .FirstOrDefaultAsync();

        //        int nextNumber = lastCustomer != null ? int.Parse(lastCustomer.CustomerId.Substring(8)) + 1 : 1;
        //        string newCustomerID = $"IT030300{nextNumber:D2}";

        //        var newCompany = new Company
        //        {
        //            CustomerId = newCustomerID,
        //            CompanyName = CompanyAccountDTO.CompanyName,
        //            TaxCode = CompanyAccountDTO.TaxCode,
        //            CompanyAccount = CompanyAccountDTO.CompanyAccount,
        //            AccountIssuedDate = CompanyAccountDTO.AccountIssuedDate,
        //            CPhoneNumber = CompanyAccountDTO.CPhoneNumber,
        //            CAddress = CompanyAccountDTO.CAddress,
        //            CustomerType = CompanyAccountDTO.CustomerType,
        //            ServiceType = CompanyAccountDTO.ServiceType,
        //            ContractNumber = CompanyAccountDTO.ContractNumber,
        //        };

        //        var newAccount = new Account
        //        {
        //            CustomerId = newCustomerID,
        //            RootAccount = CompanyAccountDTO.RootAccount,
        //            RootName = CompanyAccountDTO.RootName,
        //            RPhoneNumber = CompanyAccountDTO.RPhoneNumber,
        //            OperatingStatus = CompanyAccountDTO.OperatingStatus,
        //            DateOfBirth = CompanyAccountDTO.DateOfBirth,
        //            Gender = CompanyAccountDTO.Gender,
        //        };

        //        _context.Companies.Add(newCompany);
        //        _context.Accounts.Add(newAccount);

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        // **BƯỚC 2: Gửi email sau khi commit**
        //        try
        //        {
        //            string resetToken = GenerateResetToken(CompanyAccountDTO.RootAccount);
        //            var db = _redis.GetDatabase();
        //            await db.StringSetAsync($"reset_password:{resetToken}", CompanyAccountDTO.RootAccount, TimeSpan.FromMinutes(30));

        //            string resetLink = $"https://yourwebsite.com/reset-password?token={resetToken}";
        //            await SendResetPasswordEmail(CompanyAccountDTO.RootAccount, resetLink);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError($"Lỗi gửi email hoặc lưu token vào Redis: {ex.Message}");
        //        }

        //        return newCustomerID;
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        _logger.LogError($"Lỗi hệ thống: {ex.Message}");
        //        return "Lỗi hệ thống, vui lòng thử lại sau.";
        //    }
        //}


        //private string GenerateResetToken(string email)
        //{
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var key = Encoding.ASCII.GetBytes("your_secret_key_here");

        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) }),
        //        Expires = DateTime.UtcNow.AddMinutes(30), // Hết hạn sau 30 phút
        //        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        //    };

        //    var token = tokenHandler.CreateToken(tokenDescriptor);
        //    return tokenHandler.WriteToken(token);
        //}

        //private async Task SendResetPasswordEmail(string email, string resetLink)
        //{
        //    string subject = "Thiết lập mật khẩu tài khoản";
        //    string body = $"Vui lòng nhấn vào <a href='{resetLink}'>đây</a> để đặt lại mật khẩu.";

        //    var mailRequest = new MailRequest
        //    {
        //        ToEmail = email,
        //        Subject = subject,
        //        Body = body
        //    };

        //    await _emailService.SendEmailAsync(mailRequest);
        //}


    }
}

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
                        join q in _context.Payments on b.Contractnumber equals q.Contractnumber
                        select new CompanyAccountDTO
                        {
                            CustomerId = c.Customerid,
                            CompanyName = c.Companyname,
                            TaxCode = c.Taxcode,
                            CompanyAccount = c.Companyaccount,
                            AccountIssuedDate = c.Accountissueddate,
                            CPhoneNumber = c.Cphonenumber,
                            CAddress = c.Caddress,
                            CustomerType = c.Customertype,
                            ServiceType = h.ServiceTypename,
                            ContractNumber = b.Contractnumber,
                            RootAccount = a.Rootaccount,
                            RootName = a.Rootname,
                            RPhoneNumber = a.Rphonenumber,
                            OperatingStatus = c.Operatingstatus,
                            DateOfBirth = a.Dateofbirth,
                            Gender = a.Gender,
                            Startdate = b.Startdate,
                            Enddate = b.Enddate,
                            Amount = q.Amount,
                            Original = b.Original,
                        };

            if (!string.IsNullOrEmpty(req.Keyword))
            {
                query = query.Where(c =>
                    c.CompanyName.Contains(req.Keyword) ||
                    c.CustomerId.Contains(req.Keyword) ||
                    c.CompanyAccount.Contains(req.Keyword) ||
                    c.TaxCode.Contains(req.Keyword));
            }

            if (req.Category == "Bình thường")
            {
                query = query.Where(c => !c.CustomerType);
            }
            else if (req.Category == "Vip")
            {
                query = query.Where(c => c.CustomerType);
            }

            // Lấy toàn bộ dữ liệu trước khi group
            var rawList = await query.ToListAsync();

            // Gom nhóm theo Original (nếu có) hoặc ContractNumber
            var grouped = rawList
                .GroupBy(x => x.Original ?? x.ContractNumber)
                .Select(g => new CompanyAccountDTO
                {
                    CustomerId = g.First().CustomerId,
                    CompanyName = g.First().CompanyName,
                    TaxCode = g.First().TaxCode,
                    CompanyAccount = g.First().CompanyAccount,
                    AccountIssuedDate = g.First().AccountIssuedDate,
                    CPhoneNumber = g.First().CPhoneNumber,
                    CAddress = g.First().CAddress,
                    CustomerType = g.First().CustomerType,
                    ServiceType = g.First().ServiceType,
                    ContractNumber = g.Key, // dùng ContractNumber gốc
                    RootAccount = g.First().RootAccount,
                    RootName = g.First().RootName,
                    RPhoneNumber = g.First().RPhoneNumber,
                    OperatingStatus = g.First().OperatingStatus,
                    DateOfBirth = g.First().DateOfBirth,
                    Gender = g.First().Gender,
                    Startdate = g.Min(x => x.Startdate), // Lấy nhỏ nhất trong các lần gia hạn
                    Enddate = g.Max(x => x.Enddate),     // Lấy lớn nhất
                    Amount = g.Sum(x => x.Amount),       // Tổng tiền thanh toán của tất cả hợp đồng
                    Original = null // không cần hiển thị Original nữa
                })
                .ToList();

            // Phân trang
            var totalRow = grouped.Count;
            var pagedResult = grouped
                .OrderByDescending(c => c.CustomerId)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToList();

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
                    existingAccount.Dateofbirth = CompanyAccountDTO.DateOfBirth;
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
                        join b in _context.Contracts on c.Customerid equals b.Customerid
                        join h in _context.Payments on b.Contractnumber equals h.Contractnumber
                        select new CompanyAccountDTO
                        {
                            CustomerId = c.Customerid,
                            CompanyName = c.Companyname,
                            TaxCode = c.Taxcode,
                            CompanyAccount = c.Companyaccount,
                            AccountIssuedDate = c.Accountissueddate,
                            OperatingStatus = c.Operatingstatus,
                            CustomerType = c.Customertype,
                            ContractNumber = b.Contractnumber,
                            Startdate = b.Startdate,
                            Enddate = b.Enddate,
                            Amount = h.Amount,
                        };

            if (!string.IsNullOrEmpty(req.Keyword))
            {
                query = query.Where(c =>
                    c.CompanyName.Contains(req.Keyword) ||
                    c.CustomerId.Contains(req.Keyword) ||
                    c.CompanyAccount.Contains(req.Keyword) ||
                    c.TaxCode.Contains(req.Keyword));
            }

            if (req.Category == "Bình thường")
            {
                query = query.Where(c => c.CustomerType == false);
            }
            else if (req.Category == "Vip")
            {
                query = query.Where(c => c.CustomerType == true);
            }

            var data = await query.ToListAsync();
            if (!data.Any())
            {
                throw new Exception("Không có dữ liệu để xuất!");
            }

            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
            {
                writer.WriteLine("STT,Mã khách hàng,Tên công ty,Mã số thuế,Tài khoản root,Ngày cấp tài khoản,Trạng thái");

                int stt = 1;
                foreach (var item in data)
                {
                    string ngayCap = item.AccountIssuedDate?.ToString("dd/MM/yyyy") ?? "";
                    string trangThai = item.OperatingStatus ? "Hoạt động" : "Không hoạt động";

                    writer.WriteLine($"{stt},{item.CustomerId},{item.CompanyName},{item.TaxCode},{item.CompanyAccount},{ngayCap},{trangThai}");
                    stt++;
                }

                writer.Flush();
                return memoryStream.ToArray(); // ✅ Trả về `byte[]`
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
                        Customertype = CompanyAccountDTO.CustomerType,
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
                        Dateofbirth = CompanyAccountDTO.DateOfBirth,
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
                        Startdate = CompanyAccountDTO.Startdate,
                        Enddate = CompanyAccountDTO.Enddate,
                        ServiceTypeid = serviceType.Id,
                        Customerid = newCustomerID
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using System.Text;
using WebApi.Content;
using WebApi.DTO;
using WebApi.Helper;
using WebApi.Models;
using WebApi.Service.Client;

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

        //lấy những công ty đã chính thức isactive = true
        public async Task<PagingResult<CompanyAccountDTO>> GetAllCompany(GetListCompanyPaging req)
        {

            var query = from c in _context.Companies
                        join a in _context.Accounts on c.Customerid equals a.Customerid
                        join b in _context.Contracts on c.Customerid equals b.Customerid
                        join h in _context.ServiceTypes on b.ServiceTypeid equals h.Id where c.IsActive == true
                        group new { c, a, b, h } by new
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
                            b.IsActive,
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
                            IsActive = g.Key.IsActive,
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

                var sql = $"UPDATE CONTRACTS SET IS_ACTIVE = @p0 WHERE CustomerId = @p1";

                int rowsAffected = _context.Database.ExecuteSqlInterpolated(
                    $"UPDATE CONTRACTS SET IS_ACTIVE = {Tinhtrang} WHERE CustomerId = {CustomerID}");

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

                    if (_context.Contracts.Any(s => s.Contractnumber == CompanyAccountDTO.ContractNumber && s.Customerid != CompanyAccountDTO.CustomerId))
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

        //xuất những công ty đã chính thức
        public async Task<byte[]> ExportToCsv(ExportRequestDTO req)
        {
            var query = from c in _context.Companies
                        join a in _context.Accounts on c.Customerid equals a.Customerid
                        join b in _context.Contracts on c.Customerid equals b.Customerid
                        where c.IsActive == true
                        group new { c, a, b } by new
                        {
                            c.Customerid,
                            c.Companyname,
                            c.Cphonenumber,
                            c.Taxcode,
                            c.Companyaccount,
                            c.Accountissueddate,
                            b.IsActive,
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
                            IsActive = g.Key.IsActive,
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

                    string trangThai = item.IsActive ? "Hoạt động" : "Không hoạt động";

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
        public async Task<List<ServiceTypeDTO2>> GetListServiceID()
        {
            // Thực hiện join giữa ServiceGroups và Regulations để lấy thêm thông tin về giá
            var regulationsWithGroups = await (from serviceGroup in _context.ServiceTypes
                                               join regulation in _context.Regulations
                                               on serviceGroup.ServiceGroupid equals regulation.ServiceGroupid
                                               select new ServiceTypeDTO2
                                               {
                                                   ServiceGroupid = serviceGroup.ServiceGroupid,
                                                   ServiceTypeNames = serviceGroup.ServiceTypename,
                                                   Price = regulation.Price
                                               }).ToListAsync();

            return regulationsWithGroups;
        }
        
        //Lưu thông tin công ty tạm thời. chờ boos ký. 
        public async Task<string?> SaveContractStatusAsync(CompanyContractDTOs dto)
        {
            if (dto == null)
            {
                return "Dữ liệu không hợp lệ.";
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var staff = _context.Staff.FirstOrDefault(s => s.Staffid == dto.ChangedBy);
                    if (staff == null)
                    {
                        return $"Nhân viên với mã nhân viên = {dto.ChangedBy} không tồn tại";
                    }

                    if (_context.Companies.Any(s => s.Cphonenumber == dto.CPhoneNumber) ||
                        _context.Accounts.Any(a => a.Rphonenumber == dto.CPhoneNumber))
                    {
                        return "Số điện thoại đã tồn tại trong hệ thống. Vui lòng kiểm tra lại.";
                    }

                    if (_context.Companies.Any(s => s.Companyaccount == dto.CompanyAccount) ||
                        _context.Accounts.Any(a => a.Rootaccount == dto.CompanyAccount))
                    {
                        return "Email đã tồn tại trong hệ thống. Vui lòng kiểm tra lại.";
                    }

                    if (_context.Companies.Any(s => s.Taxcode == dto.TaxCode))
                    {
                        return "Mã số thuế đã tồn tại trong hệ thống! Vui lòng kiểm tra lại.";
                    }

                    if (_context.Companies.Any(s => s.Cphonenumber == dto.RPhoneNumber) ||
                        _context.Accounts.Any(a => a.Rphonenumber == dto.RPhoneNumber))
                    {
                        return "Số điện thoại đã tồn tại trong hệ thống. Vui lòng kiểm tra lại.";
                    }

                    if (_context.Companies.Any(s => s.Companyaccount == dto.RootAccount) ||
                        _context.Accounts.Any(a => a.Rootaccount == dto.RootAccount))
                    {
                        return "Email đã tồn tại trong hệ thống. Vui lòng kiểm tra lại.";
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
                        Companyname = dto.CompanyName,
                        Taxcode = dto.TaxCode,
                        Companyaccount = dto.CompanyAccount,
                        //Accountissueddate = dto.AccountIssuedDate,
                        Cphonenumber = dto.CPhoneNumber,
                        Caddress = dto.CAddress,
                        IsActive = false,
                    };

                    _context.Companies.Add(newCompany);

                    var newAccount = new Account
                    {
                        Customerid = newCustomerID,
                        Rootaccount = dto.RootAccount,
                        Rootname = dto.RootName,
                        Rphonenumber = dto.RPhoneNumber,
                        Dateofbirth = (DateTime)dto.DateOfBirth!,
                        Gender = dto.Gender,
                    };
                    _context.Accounts.Add(newAccount);


                    var lastContract = _context.Contracts
                .OrderByDescending(c => c.Contractnumber)
                .FirstOrDefault();
                    if (!_context.ServiceTypes.Any(s => s.ServiceTypename == dto.ServiceType))
                    {
                        return "Loại dịch vụ không tồn tại trong hệ thống. Vui lòng kiểm tra lại.";
                    }

                    int nextContractNumber = lastContract != null ? int.Parse(lastContract.Contractnumber.Substring(2)) + 1 : 1;
                    string newContractNumber = $"SV{nextContractNumber:D4}";

                    var serviceType = _context.ServiceTypes
    .FirstOrDefault(st => st.ServiceTypename == dto.ServiceType);

                    if (serviceType == null)
                    {
                        return $"Loại dịch vụ '{dto.ServiceType}' không tồn tại.";
                    }

                    var newContract = new Contract
                    {
                        Contractnumber = newContractNumber,
                        Startdate = (DateTime)dto.Startdate!,
                        Enddate = (DateTime)dto.Enddate!,
                        ServiceTypeid = serviceType.Id,
                        Customerid = newCustomerID,
                        Customertype = dto.CustomerType,
                        IsActive = false,
                        Constatus = 0
                    };
                    var newPayment = new Payment
                    {
                        Contractnumber = newContractNumber,
                        Amount = dto.Amount,
                        Paymentstatus = false,
                    };
                    _context.Payments.Add(newPayment);
                    _context.Contracts.Add(newContract);

                    var newContractfile = new ContractFile
                    {
                        Contractnumber = newContractNumber,
                        ConfileName = dto.ConfileName, 
                        FilePath = dto.FilePath,
                        UploadedAt = DateTime.Now,
                        FileStatus = 0,
                    };

                    var newContractStatusHistory = new ContractStatusHistory
                    {
                        Contractnumber = newContractNumber,
                        OldStatus =0,
                        NewStatus = 0, 
                        ChangedAt = DateTime.Now,
                        ChangedBy = dto.ChangedBy,
                    };
                    _context.ContractFiles.Add(newContractfile);
                    _context.ContractStatusHistories.Add(newContractStatusHistory);

                    _context.SaveChanges();
                    transaction.Commit();

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
        
        //Danh sách chờ boss ký,ký xong và chờ client ký
        //nếu đã ký thì gửi cho client 
        // ký hoàn tất thì duyệt hợp đồng tạo tài khoản
        public async Task<PagingResult<CompanyContractDTOs>> GetListPending(GetListCompanyPaging req)
        {
            var latestFiles = from f in _context.ContractFiles
                                group f by f.Contractnumber into g
                                select new
                                {
                                    Contractnumber = g.Key,
                                    LatestTime = g.Max(x => x.UploadedAt)
                                };

            var fileJoin = from h in _context.ContractFiles
                            join lf in latestFiles
                            on new { h.Contractnumber, h.UploadedAt } equals new { lf.Contractnumber, UploadedAt = lf.LatestTime }
                            select h;

            var query = from c in _context.Companies
                        join a in _context.Accounts on c.Customerid equals a.Customerid
                        join b in _context.Contracts on c.Customerid equals b.Customerid
                        join h in fileJoin on b.Contractnumber equals h.Contractnumber
                        where c.IsActive == false &&
                                //(b.Constatus=="Chưa ký"||b.Constatus == "Đã ký" ||b.Constatus=="Chờ client ký"|| b.Constatus == "Ký hoàn tất" || b.Constatus == "Đã thanh toán")
                                (b.Constatus==0||b.Constatus == 1 ||b.Constatus==2|| b.Constatus == 3 || b.Constatus == 4)
                        select new CompanyContractDTOs
                        {
                            ContractNumber = b.Contractnumber,
                            CustomerId = c.Customerid,
                            CompanyName = c.Companyname,
                            TaxCode = c.Taxcode,
                            CompanyAccount = c.Companyaccount,
                            AccountIssuedDate = c.Accountissueddate,
                            CPhoneNumber = c.Cphonenumber,
                            CAddress = c.Caddress,
                            RootAccount = a.Rootaccount,
                            RootName = a.Rootname,
                            RPhoneNumber = a.Rphonenumber,
                            DateOfBirth = a.Dateofbirth,
                            Gender = a.Gender,
                            FilePath = h.FilePath,
                            ConfileName = h.ConfileName,
                            Constatus = b.Constatus
                        };

            var totalRow = await query.CountAsync();
            var pagedResult = await query
                .OrderByDescending(c => c.CustomerId)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

            var pageCount = (int)Math.Ceiling(totalRow / (double)req.PageSize);

            return new PagingResult<CompanyContractDTOs>
            {
                Results = pagedResult,
                CurrentPage = req.Page,
                RowCount = totalRow,
                PageSize = req.PageSize,
                PageCount = pageCount
            };

        }

        //Boss  ký 
        public async Task<string> UploadDirectorSigned(SignAdminRequest request)
        {
            try
            {
                var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.Contractnumber == request.ContractNumber);
                if (contract == null)
                    return "Không tìm thấy hợp đồng tương ứng";

                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Customerid == contract.Customerid);

                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Lưu trạng thái cũ để lưu vào lịch sử
                    var oldStatus = contract.Constatus;

                    // Cập nhật trạng thái hợp đồng
                    contract.Constatus = 1;
                    _context.Contracts.Update(contract);

                    // Thêm thông tin file (không lưu file nữa, chỉ lưu tên và đường dẫn hiện tại)
                    var newContractFile = new ContractFile
                    {
                        Contractnumber = request.ContractNumber,
                        ConfileName = Path.GetFileName(request.FilePath), // Lấy tên file từ đường dẫn
                        FilePath = request.FilePath,
                        UploadedAt = DateTime.Now,
                        FileStatus = 1,
                    };
                    _context.ContractFiles.Add(newContractFile);

                    // Thêm lịch sử trạng thái hợp đồng
                    var newContractStatusHistory = new ContractStatusHistory
                    {
                        Contractnumber = contract.Contractnumber,
                        OldStatus = oldStatus,
                        NewStatus = 1,
                        ChangedAt = DateTime.Now,
                        ChangedBy = request.StaffId,
                    };
                    _context.ContractStatusHistories.Add(newContractStatusHistory);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return "Cập nhật trạng thái và thông tin file thành công";
                }
                catch (Exception dbEx)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"[DB ERROR] {dbEx.Message}");
                    return "Lỗi khi lưu dữ liệu vào cơ sở dữ liệu.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GENERAL ERROR] {ex.Message}");
                return "Lỗi khi xử lý file hoặc kết nối cơ sở dữ liệu.";
            }
        }

        //cập nhật sau khi gửi cho client 
        public async Task<string> UploadSendclient(SignAdminRequest request)
        {
            try
            {
                var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.Contractnumber == request.ContractNumber);
                if (contract == null)
                    return "Không tìm thấy hợp đồng tương ứng";

                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Customerid == contract.Customerid);

                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Lưu trạng thái cũ để lưu vào lịch sử
                    var oldStatus = contract.Constatus;

                    // Cập nhật trạng thái hợp đồng
                    contract.Constatus = 2;
                    _context.Contracts.Update(contract);

                    // Thêm thông tin file (không lưu file nữa, chỉ lưu tên và đường dẫn hiện tại)
                    var newContractFile = new ContractFile
                    {
                        Contractnumber = request.ContractNumber,
                        ConfileName = Path.GetFileName(request.FilePath), // Lấy tên file từ đường dẫn
                        FilePath = request.FilePath,
                        UploadedAt = DateTime.Now,
                        FileStatus = 2,
                    };
                    _context.ContractFiles.Add(newContractFile);

                    // Thêm lịch sử trạng thái hợp đồng
                    var newContractStatusHistory = new ContractStatusHistory
                    {
                        Contractnumber = contract.Contractnumber,
                        OldStatus = oldStatus,
                        NewStatus = 2,
                        ChangedAt = DateTime.Now,
                        ChangedBy = request.StaffId,
                    };
                    _context.ContractStatusHistories.Add(newContractStatusHistory);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return "Cập nhật trạng thái và thông tin file thành công";
                }
                catch (Exception dbEx)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"[DB ERROR] {dbEx.Message}");
                    return "Lỗi khi lưu dữ liệu vào cơ sở dữ liệu.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GENERAL ERROR] {ex.Message}");
                return "Lỗi khi xử lý file hoặc kết nối cơ sở dữ liệu.";
            }
        }
        //Cập nhật cũng như lưu công ty đã chính thức
        public async Task<string?> Insert(SignAdminRequest request)
        {
            if (request == null)
                return "Dữ liệu không hợp lệ.";

            var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.Contractnumber == request.ContractNumber);
            if (contract == null)
                return "Không tìm thấy hợp đồng tương ứng";

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Customerid == contract.Customerid);
            var account = await _context.Accounts.FirstOrDefaultAsync(c => c.Customerid == contract.Customerid);

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var oldStatus = contract.Constatus;

                    // Cập nhật dữ liệu
                    company.IsActive = true;
                    company.Accountissueddate = DateTime.Now;
                    contract.Constatus = 5;
                    contract.IsActive = true;

                    _context.Companies.Update(company);
                    _context.Accounts.Update(account);
                    _context.Contracts.Update(contract);

                    var newContractFile = new ContractFile
                    {
                        Contractnumber = request.ContractNumber,
                        ConfileName = Path.GetFileName(request.FilePath),
                        FilePath = request.FilePath,
                        UploadedAt = DateTime.Now,
                        FileStatus = 5
                    };
                    _context.ContractFiles.Add(newContractFile);

                    var newContractStatusHistory = new ContractStatusHistory
                    {
                        Contractnumber = contract.Contractnumber,
                        OldStatus = oldStatus,
                        NewStatus = 5,
                        ChangedAt = DateTime.Now,
                        ChangedBy = request.StaffId,
                    };
                    _context.ContractStatusHistories.Add(newContractStatusHistory);

                    string generatedPassword = GenerateRandomPassword();
                    string hashedPassword = HashPassword(generatedPassword);

                    var newLogin = new Loginclient
                    {
                        Customerid = company.Customerid,
                        Username = account.Rphonenumber,
                        Passwordclient = hashedPassword
                    };
                    _context.Loginclients.Add(newLogin);

                    // Gửi email TRƯỚC khi commit DB
                    await SendPasswordEmail(account.Rootaccount, generatedPassword);
                    string contractLink = GenerateContractLink(request.FilePath);
                    await _emailService.SendFinalContractToCustomer(account.Rootaccount, contractLink);

                    // Nếu gửi email thành công, mới commit DB
                    _context.SaveChanges();
                    await transaction.CommitAsync();

                    return company.Customerid;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
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

        private string GenerateContractLink(string filePath)
        {
            // Ví dụ: filePath = "wwwroot/contracts/final/hd123.pdf"
            var fileName = Path.GetFileName(filePath);
            return $"https://localhost:7190/signed-contracts/{fileName}";

        }

    }
}

using Microsoft.EntityFrameworkCore;
using WebApi.DTO;
using WebApi.Models;
using WebApi.Service.Client;

namespace WebApi.Service.Admin
{
    public class ContractsManagementService
    {
        private readonly ManagementDbContext _context;

        public ContractsManagementService(ManagementDbContext context)
        {
            _context = context;
        }

        public async Task<PagingResult<CompanyAccountDTO>> GetAllCompany(GetListCompanyPaging req)
        {
            var query = from c in _context.Companies
                        join a in _context.Accounts on c.Customerid equals a.Customerid
                        join b in _context.Contracts on c.Customerid equals b.Customerid
                        join h in _context.ServiceTypes on b.ServiceTypeid equals h.Id
                        join q in _context.Payments on b.Contractnumber equals q.Contractnumber
                        where c.IsActive == true
                        select new CompanyAccountDTO
                        {
                            CustomerId = c.Customerid,
                            CompanyName = c.Companyname,
                            TaxCode = c.Taxcode,
                            CompanyAccount = c.Companyaccount,
                            AccountIssuedDate = c.Accountissueddate,
                            CPhoneNumber = c.Cphonenumber,
                            CAddress = c.Caddress,
                            CustomerType = b.Customertype,
                            ServiceType = h.ServiceTypename,
                            ContractNumber = b.Contractnumber,
                            RootAccount = a.Rootaccount,
                            RootName = a.Rootname,
                            RPhoneNumber = a.Rphonenumber,
                            IsActive = b.IsActive,
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
                    IsActive = g.First().IsActive,
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
        public string? InsertExtend(ContractDTO contractDTO, string id)
        {
            if (contractDTO == null || contractDTO.chooseMonth <= 0)
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

                    var oldContract = _context.Contracts
                        .FirstOrDefault(c => c.Contractnumber == contractDTO.ContractNumber);

                    if (oldContract == null)
                    {
                        return $"Hợp đồng với mã {contractDTO.ContractNumber} không tồn tại.";
                    }

                    // Tạo số hợp đồng mới
                    var lastContract = _context.Contracts
                        .OrderByDescending(c => c.Contractnumber)
                        .FirstOrDefault();

                    int nextContractNumber = lastContract != null
                        ? int.Parse(lastContract.Contractnumber.Substring(2)) + 1
                        : 1;

                    string newContractNumber = $"SV{nextContractNumber:D4}";

                    // Tìm loại dịch vụ
                    var serviceType = _context.ServiceTypes
                        .FirstOrDefault(st => st.ServiceTypename == contractDTO.ServiceType);

                    if (serviceType == null)
                    {
                        return $"Loại dịch vụ '{contractDTO.ServiceType}' không tồn tại.";
                    }

                    DateTime newStartDate;
                    DateTime newEndDate;
                    string? originalContract = null;
                    // Tìm toàn bộ chuỗi hợp đồng gốc + gia hạn
                    var contractChain = _context.Contracts
                        .Where(c => c.Contractnumber == contractDTO.ContractNumber || c.Original == contractDTO.ContractNumber)
                        .ToList();

                    if (!contractChain.Any())
                    {
                        return $"Hợp đồng với mã {contractDTO.ContractNumber} không tồn tại.";
                    }

                    // Tìm hợp đồng có ngày kết thúc lớn nhất
                    var latestContract = contractChain.OrderByDescending(c => c.Enddate).First();

                    if (latestContract.Enddate < DateTime.Today)
                    {
                        // Hợp đồng đã hết hạn → tạo mới
                        newStartDate = DateTime.Today;
                        newEndDate = newStartDate.AddMonths(contractDTO.chooseMonth);
                        originalContract = null;
                    }
                    else
                    {
                        // Hợp đồng còn hạn → gia hạn từ ngày hết hạn cũ + 1 đến ngày người dùng chọn
                        newStartDate = latestContract.Enddate.AddDays(1);
                        newEndDate = contractDTO.Enddate;
                        originalContract = latestContract.Contractnumber;
                    }
                    var newContract = new Contract
                    {
                        Contractnumber = newContractNumber,
                        Startdate = newStartDate,
                        Enddate = newEndDate,
                        ServiceTypeid = serviceType.Id,
                        Customerid = contractDTO.CustomerId,
                        Original = originalContract
                    };

                    var newPayment = new Payment
                    {
                        Contractnumber = newContractNumber,
                        Amount = contractDTO.Amount,
                        Paymentstatus = false
                    };

                    _context.Contracts.Add(newContract);
                    _context.Payments.Add(newPayment);
                    _context.SaveChanges();
                    transaction.Commit();

                    return null;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Lỗi hệ thống: {ex.Message}");
                    return "Lỗi hệ thống, vui lòng thử lại sau.";
                }
            }
        }

        public async Task<string?> InsertContract(CompanyAccountDTO CompanyAccountDTO, string id)
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

                    var lastContract = _context.Contracts
                .OrderByDescending(c => c.Contractnumber)
                .FirstOrDefault();
                    int nextContractNumber = lastContract != null ? int.Parse(lastContract.Contractnumber.Substring(2)) + 1 : 1;
                    string newContractNumber = $"SV{nextContractNumber:D4}";

                    var serviceType = _context.ServiceTypes
    .FirstOrDefault(st => st.ServiceTypename == CompanyAccountDTO.ServiceType);

                    if (serviceType == null)
                    {
                        return $"Loại dịch vụ '{CompanyAccountDTO.ServiceType}' không tồn tại.";
                    }
                    // ✅ Kiểm tra khách hàng đã có dịch vụ này chưa
                    var existingContract = _context.Contracts
                        .Where(c => c.Customerid == CompanyAccountDTO.CustomerId
                                 && c.ServiceTypeid == serviceType.Id
                                 && c.Enddate >= DateTime.Now)
                        .FirstOrDefault();

                    if (existingContract != null)
                    {
                        return $"Khách hàng đã có hợp đồng với loại dịch vụ '{CompanyAccountDTO.ServiceType}' đang còn hiệu lực.";
                    }
                    var newContract = new Contract
                    {
                        Contractnumber = newContractNumber,
                        Startdate = (DateTime)CompanyAccountDTO.Startdate!,
                        Enddate = (DateTime)CompanyAccountDTO.Enddate!,
                        ServiceTypeid = serviceType.Id,
                        Customerid = CompanyAccountDTO.CustomerId,
                        Constatus = "Đã hoàn tất"
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

                    return newContractNumber;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Lỗi hệ thống: {ex.Message}");
                    return "Lỗi hệ thống, vui lòng thử lại sau.";
                }
            }
        }

        public string? InsertUpgrade(ContractDTO contractDTO, string id)
        {
            if (contractDTO == null)
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
                        return $"Nhân viên với mã nhân viên = {id} không tồn tại.";
                    }

                    var existingContract = _context.Contracts.FirstOrDefault(c => c.Contractnumber == contractDTO.ContractNumber);
                    if (existingContract == null)
                    {
                        return "Hợp đồng không tồn tại.";
                    }

                    // Kiểm tra mã khách hàng có tồn tại trong bảng hợp đồng không
                    if (!_context.Contracts.Any(s => s.Customerid == contractDTO.CustomerId))
                    {
                        return "Mã khách hàng không tồn tại trong hệ thống.";
                    }

                    // Cập nhật loại khách hàng
                    existingContract.Customertype = contractDTO.Customertype;
                    // Cập nhật loại khách hàng cho các hợp đồng phụ có ORIGINAL là contract gốc
                    var relatedContracts = _context.Contracts
                        .Where(c => c.Original == contractDTO.ContractNumber)
                        .ToList();

                    foreach (var contract in relatedContracts)
                    {
                        contract.Customertype = contractDTO.Customertype;
                    }

                    _context.SaveChanges();


                    var newPayment = new Payment
                    {
                        Contractnumber = contractDTO.ContractNumber,
                        Amount = contractDTO.Amount,
                        Paymentstatus = false
                    };

                    _context.Payments.Add(newPayment);
                    _context.SaveChanges();

                    transaction.Commit();
                    return null; // Trả về null khi thành công
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Lỗi hệ thống: {ex.Message}");
                    return "Lỗi hệ thống, vui lòng thử lại sau.";
                }
            }
        }
        public async Task<List<Endow>> GetListEndow(string id)
        {
            var endowList = await (from endow in _context.Endows
                                   where endow.ServiceGroupid == id
                                   select new Endow
                                   {
                                       Endowid = endow.Endowid,
                                       ServiceGroupid = endow.ServiceGroupid,
                                       Discount = endow.Discount,
                                       Startdate = endow.Startdate,
                                       Enddate = endow.Enddate,
                                       Duration = endow.Duration,
                                   }).ToListAsync();
            return endowList;
        }

        //lấy thông tin cty search để tạo phiếu yêu cầu
        //đã thêm kiểm tra điều kiện hạn hợp đồng còn, vaf  hoat dong.
        //vì ở đây chỉ lấy thông tin cty để insert nên không cần lấy theo nhiều loại dịch vụ hợp đồng. 
        public async Task<List<CompanyAccountDTO>> GetAllInfor(string req)
        {
            req = req?.Trim().ToLower();

            var query = from c in _context.Companies
                        join a in _context.Accounts on c.Customerid equals a.Customerid
                        join h in _context.Contracts
                        on c.Customerid equals h.Customerid
                        join q in _context.ServiceTypes on h.ServiceTypeid equals q.Id
                        where (
                             (string.IsNullOrEmpty(req) ||
                              c.Customerid.ToLower().Contains(req) ||
                              c.Companyname.ToLower().Contains(req) ||
                              c.Taxcode.ToLower().Contains(req))
                              && h.IsActive == true
                              && h.Enddate >= DateTime.Now
                         )
                        group new { c, a } by c.Customerid into g
                        select new CompanyAccountDTO
                        {
                            CustomerId = g.First().c.Customerid,
                            CompanyName = g.First().c.Companyname,
                            TaxCode = g.First().c.Taxcode,
                            CompanyAccount = g.First().c.Companyaccount,
                            //AccountIssuedDate = c.Accountissueddate,
                            CPhoneNumber = g.First().c.Cphonenumber,
                            CAddress = g.First().c.Caddress,
                            //CustomerType = h.Customertype,
                            //ServiceType = q.ServiceTypename,
                            //ContractNumber = h.Contractnumber,
                            RootAccount = g.First().a.Rootaccount,
                            RootName = g.First().a.Rootname,
                            RPhoneNumber = g.First().a.Rphonenumber,
                            Gender = g.First().a.Gender,
                            DateOfBirth = g.First().a.Dateofbirth,
                        };

            return await query.ToListAsync();
        }
    }
}

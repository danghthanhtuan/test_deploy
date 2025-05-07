using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using System.Net;
using WebApi.DTO;
using WebApi.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebApi.Service.Client
{
    public class RequirementService
    {
        private readonly ManagementDbContext _context;
        public RequirementService(ManagementDbContext context)
        {
            _context = context;
        }

        //public async Task<PagingResult<Requirement_Company>> GetAllRequest(GetListReq req)
        //{

        //    // Truy vấn dữ liệu với điều kiện CustomerId
        //    var query = from c in _context.Companies
        //                join a in _context.Accounts on c.Customerid equals a.Customerid
        //                join r in _context.Requirements on c.Customerid equals r.Customerid
        //                join b in _context.Contracts on c.Customerid equals b.Customerid
        //                where c.Customerid == req.Cutomer
        //                select new Requirement_Company
        //                {
        //                    RequirementsId = r.Requirementsid,
        //                    Support = r.SupportName,
        //                    RequirementsStatus = r.Requirementsstatus,
        //                    DateOfRequest = r.Dateofrequest,
        //                    DescriptionOfRequest = r.Descriptionofrequest,
        //                    CustomerId = c.Customerid,
        //                    CompanyName = c.Companyname,
        //                    TaxCode = c.Taxcode,
        //                    CompanyAccount = c.Companyaccount,
        //                    CPhoneNumber = c.Cphonenumber,
        //                    CAddress = c.Caddress,
        //                    CustomerType = c.Customertype,
        //                    ServiceType = b.ServiceTypename,
        //                    ContractNumber = b.Contractnumber,
        //                    RootAccount = a.Rootaccount,
        //                    RootName = a.Rootname,
        //                    RPhoneNumber = a.Rphonenumber
        //                };

        //    // Tổng số dòng
        //    var totalRow = await query.CountAsync();

        //    // Phân trang và sắp xếp theo RequirementsId giảm dần
        //    var sup = await query
        //        .OrderByDescending(r => r.RequirementsId)
        //        .Skip((req.Page - 1) * req.PageSize)
        //        .Take(req.PageSize)
        //        .ToListAsync();

        //    // Tổng số trang
        //    var pageCount = (int)Math.Ceiling(totalRow / (double)req.PageSize);

        //    // Trả về kết quả phân trang
        //    return new PagingResult<Requirement_Company>
        //    {
        //        Results = sup,
        //        CurrentPage = req.Page,
        //        RowCount = totalRow,
        //        PageSize = req.PageSize,
        //        PageCount = pageCount
        //    };
        //}

        public async Task<List<CompanyAccountDTO>> GetAllInfor(string req)
        {
            var query = from c in _context.Companies
                        join a in _context.Accounts on c.Customerid equals a.Customerid
                        join b in _context.Contracts on c.Customerid equals b.Customerid
                        join h in _context.ServiceTypes on b.ServiceTypeid equals h.Id
                        where c.Customerid == req
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
                        };

            return await query.ToListAsync();
        }

        public string? Insert(Requirement_C Req)
        {
            if (Req == null)
            {
                return "Dữ liệu không hợp lệ.";
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var contractNumber = _context.Contracts.FirstOrDefault(s => s.Contractnumber == Req.ContractNumber);
                    if (contractNumber == null)
                    {
                        return $"Khách hàng với mã công ty = {Req.ContractNumber} không tồn tại";
                    }
                    // Lấy tháng và năm từ chính ngày được yêu cầu (được nhập)
                    if (!Req.DateOfRequest.HasValue)
                    {
                        return "Ngày yêu cầu không hợp lệ.";
                    }

                    var requestDate = Req.DateOfRequest.Value;
                    int requestMonth = requestDate.Month;
                    int requestYear = requestDate.Year;

                    var currentMonthCount = _context.Requirements
    .Count(r => r.Contractnumber == Req.ContractNumber &&
                r.Dateofrequest.HasValue &&
                r.Dateofrequest.Value.Month == requestMonth &&
                r.Dateofrequest.Value.Year == requestYear);


                    bool isVip = contractNumber.Customertype == true;
                    int limit = isVip ? 7 : 5;

                    if (currentMonthCount >= limit)
                    {
                        return $"Khách hàng đã đạt giới hạn {limit} yêu cầu dịch vụ trong tháng {requestMonth}/{requestYear}.";
                    }
                    
                    var lastCustomer = _context.Requirements
                        .Where(c => c.Requirementsid.StartsWith("RS00"))
                        .OrderByDescending(c => c.Requirementsid)
                        .FirstOrDefault();

                    int nextNumber = 1;
                    if (lastCustomer != null)
                    {
                        string lastId = lastCustomer.Requirementsid.Substring(4);
                        if (int.TryParse(lastId, out int lastNum))
                        {
                            nextNumber = lastNum + 1;
                        }
                    }

                    string newRequirements = $"RS00{nextNumber:D2}";
                    var support = _context.SupportTypes.FirstOrDefault(st => st.SupportName == Req.Support);
                    var newReq = new Requirement
                    {
                        Requirementsid = newRequirements,
                        SupportCode = support.SupportCode,
                        Requirementsstatus = Req.RequirementsStatus,
                        Dateofrequest = Req.DateOfRequest,
                        Descriptionofrequest = Req.DescriptionOfRequest,
                        Contractnumber = Req.ContractNumber,
                    };

                    string department = Req.Support switch
                    {
                        "Hỗ trợ Cước phí" => "Hành chính",
                        "Cập nhật hợp đồng, dịch vụ" => "Hành chính",
                        "Hỗ trợ Kỹ thuật" => "Kỹ thuật",
                        "Bảo hành thiết bị" => "Kỹ thuật",
                        _ => "Chưa phân loại"
                    };

                    var assign = new Assign
                    {
                        Requirementsid = newRequirements,
                        Department = department
                    };

                    _context.Assigns.Add(assign);
                    _context.Requirements.Add(newReq);
                    _context.SaveChanges();

                    transaction.Commit();
                    return newRequirements;
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

        public async Task<List<Historyreq>> getHistory(string req)
        {
            var query = from c in _context.Requirements
                        join b in _context.Historyreqs on c.Requirementsid equals b.Requirementsid
                        where c.Requirementsid == req
                        select new Historyreq
                        {
                            Requirementsid = b.Requirementsid,
                            Descriptionofrequest = b.Descriptionofrequest,
                            Dateofupdate = b.Dateofupdate,
                            Beforstatus = b.Beforstatus,
                            Apterstatus = b.Apterstatus,
                            Staffid = b.Staffid,
                        };

            return await query.ToListAsync();
        }

        public async Task<PagingResult<Request_GroupCompany_DTO>> GetAllRequest(GetListReq req)
        {

            // Truy vấn dữ liệu với điều kiện CustomerId
            var query = (from c in _context.Companies
                         //join a in _context.Accounts on c.Customerid equals a.Customerid
                         join b in _context.Contracts on c.Customerid equals b.Customerid
                         join r in _context.Requirements on b.Contractnumber equals r.Contractnumber
                         join q in _context.SupportTypes on r.SupportCode equals q.SupportCode
                         join h in _context.Historyreqs on r.Requirementsid equals h.Requirementsid into historyJoin
                         from h in historyJoin.DefaultIfEmpty() // Left Join

                         where c.Customerid == req.Cutomer
                         select new Requirement_Company1
                         {
                             RequirementsId = r.Requirementsid,
                             Support = q.SupportName,
                             RequirementsStatus = r.Requirementsstatus,
                             DateOfRequest = r.Dateofrequest,
                             DescriptionOfRequest1 = r.Descriptionofrequest,
                             ContractNumber = b.Contractnumber,
                             Staffid = h.Staffid,
                             Descriptionofrequest2 = h.Descriptionofrequest,
                             BeforStatus = h.Beforstatus,
                             Apterstatus = h.Apterstatus,
                         }).AsQueryable().GroupBy(g => new { g.RequirementsId, g.Support, g.RequirementsStatus, g.DateOfRequest, g.DescriptionOfRequest1, g.ContractNumber }, (key, getHistory) => new Request_GroupCompany_DTO
                         {
                             Request_Group = new Request_Group()
                             {
                                 RequirementsId = key.RequirementsId,
                                 Support = key.Support,
                                 RequirementsStatus = key.RequirementsStatus,
                                 DateOfRequest = key.DateOfRequest,
                                 DescriptionOfRequest = key.DescriptionOfRequest1,
                                 ContractNumber = key.ContractNumber,
                             },
                             HistoryRequests = getHistory.Select(req => new HistoryRequests
                             {
                                 Requirementsid = req.RequirementsId,
                                 Staffid = req.Staffid,
                                 Descriptionofrequest = req.Descriptionofrequest2,
                                 BeforStatus = req.BeforStatus,
                                 Apterstatus = req.Apterstatus,
                             }).ToList(),
                         });

            // Tổng số dòng
            var totalRow = await query.CountAsync();

            // Phân trang và sắp xếp theo RequirementsId giảm dần
            var sup = await query
                .OrderByDescending(r => r.Request_Group.RequirementsId)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

            // Tổng số trang
            var pageCount = (int)Math.Ceiling(totalRow / (double)req.PageSize);

            // Trả về kết quả phân trang
            return new PagingResult<Request_GroupCompany_DTO>
            {
                Results = sup,
                CurrentPage = req.Page,
                RowCount = totalRow,
                PageSize = req.PageSize,
                PageCount = pageCount
            };
        }
        public async Task<Requirement_Company?> GetdetailRequest([FromQuery] string requestid)
        {
            var query = from c in _context.Companies
                        join a in _context.Accounts on c.Customerid equals a.Customerid
                        join b in _context.Contracts on c.Customerid equals b.Customerid
                        join r in _context.Requirements on b.Contractnumber equals r.Contractnumber
                        join h in _context.ServiceTypes on b.ServiceTypeid equals h.Id
                        join q in _context.SupportTypes on r.SupportCode equals q.SupportCode
                        where r.Requirementsid == requestid
                        select new Requirement_Company
                        {
                            RequirementsId = r.Requirementsid,
                            Support = q.SupportName,
                            RequirementsStatus = r.Requirementsstatus,
                            DateOfRequest = r.Dateofrequest,
                            DescriptionOfRequest = r.Descriptionofrequest,
                            CustomerId = c.Customerid,
                            CompanyName = c.Companyname,
                            TaxCode = c.Taxcode,
                            CompanyAccount = c.Companyaccount,
                            CPhoneNumber = c.Cphonenumber,
                            CAddress = c.Caddress,
                            CustomerType = b.Customertype,
                            ServiceType = h.ServiceTypename,
                            ContractNumber = b.Contractnumber,
                            RootAccount = a.Rootaccount,
                            RootName = a.Rootname,
                            RPhoneNumber = a.Rphonenumber
                        };

            return await query.FirstOrDefaultAsync(); // Lấy duy nhất một yêu cầu
        }

    }
}

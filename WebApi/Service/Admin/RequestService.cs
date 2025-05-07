using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Buffers;
using WebApi.DTO;
using WebApi.Models;

namespace WebApi.Service.Admin
{
    public class RequestService
    {
        private readonly ManagementDbContext _context;
        public RequestService(ManagementDbContext context)
        {
            _context = context;
        }

        //lấy tất cả yêu cầu của công ty 
        public async Task<PagingResult<Requirement_Company>> GetAllRequest(GetListReq req)
        {
            // Truy vấn dữ liệu với điều kiện CustomerId
            var query = from r in _context.Requirements
                        join h in _context.Contracts
                        on r.Contractnumber equals h.Contractnumber
                        join c in _context.Companies
                        on h.Customerid equals c.Customerid
                        join a in _context.Accounts
                        on c.Customerid equals a.Customerid
                        join s in _context.SupportTypes 
                        on r.SupportCode equals s.SupportCode
                       
                        join q in _context.ServiceTypes on h.ServiceTypeid equals q.Id
                       // where c.CustomerId == req.Cutomer
                        select new Requirement_Company
                        {
                            RequirementsId = r.Requirementsid,
                            Support = s.SupportName,
                            RequirementsStatus = r.Requirementsstatus,
                            DateOfRequest = r.Dateofrequest,
                            DescriptionOfRequest = r.Descriptionofrequest,
                            CustomerId = c.Customerid,
                            CompanyName = c.Companyname,
                            TaxCode = c.Taxcode,
                            CompanyAccount = c.Companyaccount,
                            CPhoneNumber = c.Cphonenumber,
                            CAddress = c.Caddress,
                            CustomerType = h.Customertype,
                            ServiceType = q.ServiceTypename,
                            ContractNumber = h.Contractnumber,
                            RootAccount = a.Rootaccount,
                            RootName = a.Rootname,
                            RPhoneNumber = a.Rphonenumber,
                            Startdate = h.Startdate,
                            Enddate = h.Enddate,
                        };

            // Tổng số dòng
            var totalRow = await query.CountAsync();
            // Phân trang và sắp xếp theo RequirementsId giảm dần
            var sup = await query
                .OrderByDescending(r => r.DateOfRequest)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

            // Tổng số trang
            var pageCount = (int)Math.Ceiling(totalRow / (double)req.PageSize);

            // Trả về kết quả phân trang
            return new PagingResult<Requirement_Company>
            {
                Results = sup,
                CurrentPage = req.Page,
                RowCount = totalRow,
                PageSize = req.PageSize,
                PageCount = pageCount
            };
        }

        //lấy thông tin cty search để tạo phiếu yêu cầu
        //đã thêm kiểm tra điều kiện hạn hợp đồng còn, vaf  hoat dong.
        //vì đây là yêu cầu riêng của từng hợp đồng nên để nhiều dòng trùng tên cty 
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
                              && c.Operatingstatus == true
                              && h.Enddate >= DateTime.Now
                         )
                        select new CompanyAccountDTO
                        {
                            CustomerId = c.Customerid,
                            CompanyName = c.Companyname,
                            TaxCode = c.Taxcode,
                            CompanyAccount = c.Companyaccount,
                            AccountIssuedDate = c.Accountissueddate,
                            CPhoneNumber = c.Cphonenumber,
                            CAddress = c.Caddress,
                            CustomerType = h.Customertype,
                            ServiceType = q.ServiceTypename,
                            ContractNumber = h.Contractnumber,
                            RootAccount = a.Rootaccount,
                            RootName = a.Rootname,
                            RPhoneNumber = a.Rphonenumber,
                            Gender = a.Gender,
                            DateOfBirth = a.Dateofbirth,
                        };

            return await query.ToListAsync();
        }

        //lấy thông tin cty khi click vào 1 yêu cầu xem chi tiết
        //ở đây lấy thêm hạn hợp đồng của yêu cầu đó check nếu hết hạn thì enable không cho chỉnh sửa 
        public async Task<List<Requirement_Company>> GetRequestByID(string req)
        {
            var query = from c in _context.Companies
                        join a in _context.Accounts on c.Customerid equals a.Customerid
                        join h in _context.Contracts on c.Customerid equals h.Customerid
                        join r in _context.Requirements on h.Contractnumber equals r.Contractnumber
                        join b in _context.Assigns on r.Requirementsid equals b.Requirementsid
                        where r.Requirementsid == req
                        join s in _context.SupportTypes
                        on r.SupportCode equals s.SupportCode
                        join q in _context.ServiceTypes on h.ServiceTypeid equals q.Id
                        select new Requirement_Company
                        {
                            RequirementsId = r.Requirementsid,
                            Support =s.SupportName,
                            RequirementsStatus = r.Requirementsstatus.Trim(),
                            DateOfRequest = r.Dateofrequest,
                            DescriptionOfRequest = r.Descriptionofrequest,
                            CustomerId = c.Customerid,
                            CompanyName = c.Companyname,
                            TaxCode = c.Taxcode,
                            CompanyAccount = c.Companyaccount,
                            CPhoneNumber = c.Cphonenumber,
                            CAddress = c.Caddress,
                            CustomerType = h.Customertype,
                            ServiceType = q.ServiceTypename,
                            ContractNumber = h.Contractnumber,
                            RootAccount = a.Rootaccount,
                            RootName = a.Rootname,
                            RPhoneNumber =a.Rphonenumber,
                            Department = b.Department,
                            Startdate = h.Startdate, 
                            Enddate = h.Enddate,
                            Operatingstatus = c.Operatingstatus
                        };

            return await query.ToListAsync();
        }

        //insert vì đã lấy theo lọc nên chỉ tạo cho yêu cầu có hợp đồng còn hạn và còn hoạt động
        public string? Insert(Requirement_C Req, string id)
        {
            if (Req == null)
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
                    var contractNumber = _context.Contracts.FirstOrDefault(s => s.Contractnumber == Req.ContractNumber);
                    if (contractNumber == null)
                    {
                        return $"Khách hàng với mã hợp đồng không tồn tại";
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
                        //SupportCode = Req.Support,
                        Requirementsstatus = Req.RequirementsStatus,
                        Dateofrequest = Req.DateOfRequest,
                        Descriptionofrequest = Req.DescriptionOfRequest,
                        Contractnumber = Req.ContractNumber,
                        SupportCode = support.SupportCode, 
                        //Staffid = id,   
                    };

                    _context.Requirements.Add(newReq);

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

        //cái này có check ở trên để chỉnh được cái nào còn hạn hợp đồng và hoạt động.
        public string? UpdateStatus(historyRequest historyReq)
        {
            try
            {
                if (string.IsNullOrEmpty(historyReq.Requirementsid))
                {
                    Console.WriteLine("Lỗi: RequirementsId bị null hoặc rỗng.");
                    return "Lỗi: RequirementsId bị null hoặc rỗng.";
                }
                // Kiểm tra trạng thái hoạt động của công ty
                var isOperating = (from r in _context.Requirements
                                   join h in _context.Contracts
                                   on r.Contractnumber equals h.Contractnumber
                                   join c in _context.Companies
                                   on h.Customerid equals c.Customerid
                                   where r.Requirementsid == historyReq.Requirementsid
                                   select c.Operatingstatus).FirstOrDefault();

                if (isOperating == false)
                {
                    Console.WriteLine("Lỗi: Công ty không còn hoạt động.");
                    return "Lỗi: Công ty không còn hoạt động.";
                }

                // Lấy trạng thái hiện tại của yêu cầu
                var currentStatus = _context.Requirements
                                            .Where(r => r.Requirementsid == historyReq.Requirementsid)
                                            .Select(r => r.Requirementsstatus)
                                            .FirstOrDefault();

                if (currentStatus == null)
                {
                    Console.WriteLine($"Lỗi: Không tìm thấy yêu cầu với RequirementsId: {historyReq.Requirementsid}");
                    return $"Lỗi: Không tìm thấy yêu cầu với RequirementsId: {historyReq.Requirementsid}";
                }
                // Kiểm tra nếu trạng thái mới giống trạng thái hiện tại
                if (currentStatus == historyReq.Apterstatus)
                {
                    Console.WriteLine("Trạng thái mới giống với trạng thái hiện tại. Không cần cập nhật.");
                    return "Trạng thái mới giống với trạng thái hiện tại. Không cần cập nhật.";
                }

                // Kiểm tra nếu trạng thái đã từng xuất hiện trong lịch sử
                var historyExists = _context.Historyreqs.Any(h =>
                    h.Requirementsid == historyReq.Requirementsid &&
                    h.Apterstatus == historyReq.Apterstatus
                );

                if (historyExists)
                {
                    Console.WriteLine("Trạng thái mới đã từng được sử dụng trước đây. Không được cập nhật lại.");
                    return "Trạng thái mới đã từng được sử dụng trước đây. Không được cập nhật lại.";
                }


                // Lấy bản ghi để cập nhật
                var existingRequirement = _context.Requirements.FirstOrDefault(c => c.Requirementsid == historyReq.Requirementsid);
                if (existingRequirement == null)
                {
                    Console.WriteLine($"Lỗi: Không tìm thấy yêu cầu với RequirementsId: {historyReq.Requirementsid}");
                    return $"Lỗi: Không tìm thấy yêu cầu với RequirementsId: {historyReq.Requirementsid}";
                }

                // Cập nhật trạng thái mới
                existingRequirement.Requirementsstatus = historyReq.Apterstatus;

                // Thêm lịch sử cập nhật
                var historyRecord = new Historyreq
                {
                    Requirementsid = historyReq.Requirementsid,
                    Descriptionofrequest = historyReq.Descriptionofrequest,
                    Apterstatus = historyReq.Apterstatus,
                    Staffid = historyReq.Staffid,
                    Beforstatus = currentStatus,
                    Dateofupdate = DateTime.Now
                };

                _context.Historyreqs.Add(historyRecord);

                // Lưu tất cả thay đổi
                _context.SaveChanges();

                Console.WriteLine($"Cập nhật thành công RequirementsId: {historyReq.Requirementsid}, Status: {historyReq.Apterstatus}");
                return null; // null nghĩa là thành công, bạn có thể đổi thành "Thành công" nếu thích.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi SQL khi cập nhật RequirementsId {historyReq.Requirementsid}: {ex.Message}");
                return $"Lỗi SQL khi cập nhật RequirementsId {historyReq.Requirementsid}: {ex.Message}";
            }
        }

        //đã bổ sung check tình trạng hoạt động trước khi thực hiện update yêu cầu
        //public bool UpdateStatus(historyRequest historyReq)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(historyReq.Requirementsid))
        //        {
        //            Console.WriteLine("Lỗi: RequirementsId bị null hoặc rỗng.");
        //            return false;
        //        }
        //        // Kiểm tra trạng thái hoạt động của công ty
        //        var isOperating = (from r in _context.Requirements
        //                           join c in _context.Companies
        //                           on r.Customerid equals c.Customerid
        //                           where r.Requirementsid == historyReq.Requirementsid
        //                           select c.Operatingstatus).FirstOrDefault();

        //        if (isOperating == false)
        //        {
        //            Console.WriteLine("Lỗi: Công ty không còn hoạt động.");
        //            return false;
        //        }

        //        // Lấy trạng thái hiện tại của yêu cầu
        //        var currentStatus = _context.Requirements
        //                                    .Where(r => r.Requirementsid == historyReq.Requirementsid)
        //                                    .Select(r => r.Requirementsstatus)
        //                                    .FirstOrDefault();

        //        if (currentStatus == null)
        //        {
        //            Console.WriteLine($"Lỗi: Không tìm thấy yêu cầu với RequirementsId: {historyReq.Requirementsid}");
        //            return false;
        //        }
        //        // Kiểm tra nếu trạng thái mới giống trạng thái hiện tại
        //        if (currentStatus == historyReq.Apterstatus)
        //        {
        //            Console.WriteLine("Trạng thái mới giống với trạng thái hiện tại. Không cần cập nhật.");
        //            return false;
        //        }

        //        // Kiểm tra nếu trạng thái đã từng xuất hiện trong lịch sử
        //        var historyExists = _context.Historyreqs.Any(h =>
        //            h.Requirementsid == historyReq.Requirementsid &&
        //            h.Apterstatus == historyReq.Apterstatus
        //        );

        //        if (historyExists)
        //        {
        //            Console.WriteLine("Trạng thái mới đã từng được sử dụng trước đây. Không được cập nhật lại.");
        //            return false;
        //        }


        //        // Lấy bản ghi để cập nhật
        //        var existingRequirement = _context.Requirements.FirstOrDefault(c => c.Requirementsid == historyReq.Requirementsid);
        //        if (existingRequirement == null)
        //        {
        //            Console.WriteLine($"Lỗi: Không tìm thấy yêu cầu với RequirementsId: {historyReq.Requirementsid}");
        //            return false;
        //        }

        //        // Cập nhật trạng thái mới
        //        existingRequirement.Requirementsstatus = historyReq.Apterstatus;

        //        // Thêm lịch sử cập nhật
        //        var historyRecord = new Historyreq
        //        {
        //            Requirementsid = historyReq.Requirementsid,
        //            Descriptionofrequest = historyReq.Descriptionofrequest,
        //            Apterstatus = historyReq.Apterstatus,
        //            Staffid = historyReq.Staffid,
        //            Beforstatus = currentStatus,
        //            Dateofupdate = DateTime.Now
        //        };

        //        _context.Historyreqs.Add(historyRecord);

        //        // Lưu tất cả thay đổi
        //        _context.SaveChanges();

        //        Console.WriteLine($"Cập nhật thành công RequirementsId: {historyReq.Requirementsid}, Status: {historyReq.Apterstatus}");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Lỗi SQL khi cập nhật RequirementsId {historyReq.Requirementsid}: {ex.Message}");
        //        return false;
        //    }
        //}

        //lấy tình trạng css
        public async Task<List<HistoryRequests>> getHIS(string req)
        {
            req = req?.Trim().ToLower();

            // Bản ghi ban đầu từ bảng Requirements
            var initial = from c in _context.Requirements
                          join a in _context.Assigns on c.Requirementsid equals a.Requirementsid
                          where c.Requirementsid.ToLower().Contains(req)
                          select new HistoryRequests
                          {
                              Requirementsid = c.Requirementsid,
                              Staffid = a.Staffid,
                              Descriptionofrequest = c.Descriptionofrequest,
                              //BeforStatus = "Khởi tạo",
                              Apterstatus = "Yêu cầu hỗ trợ",
                              DateOfRequest = c.Dateofrequest // hoặc tên cột tương ứng
                          };

            // Các bản ghi tiếp theo từ bảng Historyreqs
            var history = from c in _context.Requirements
                          join h in _context.Historyreqs on c.Requirementsid equals h.Requirementsid
                          where c.Requirementsid.ToLower().Contains(req)
                          select new HistoryRequests
                          {
                              Requirementsid = c.Requirementsid,
                              Staffid = h.Staffid,
                              Descriptionofrequest = h.Descriptionofrequest,
                             // BeforStatus = h.Beforstatus,
                              Apterstatus = h.Apterstatus,
                              DateOfRequest = h.Dateofupdate,
                          };

            // Gộp lại và sắp xếp theo thời gian
            var result = await initial
                .Union(history)
                .OrderBy(r => r.DateOfRequest)
                .ToListAsync();

            return result;
        }

    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading;
using WebApi.DTO;
using WebApi.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebApi.Service.Admin
{
    public class RegulationsService
    {
        private readonly ManagementDbContext _context;
        public RegulationsService(ManagementDbContext context)
        {
            _context = context;
        }
        public async Task<PagingResult<RegulationsDTO>> GetAllRegulations(GetListReq req)
        {
            // Lấy danh sách các nhóm dịch vụ cần thiết
            var regulationsWithGroups = await (from r in _context.Regulations
                                               join g in _context.ServiceGroups on r.ServiceGroupid equals g.ServiceGroupid
                                               select new
                                               {
                                                   r.ServiceGroupid,
                                                   g.GroupName,
                                                   r.Price
                                               }).ToListAsync();

            // Lấy tất cả ServiceTypes
            var allServiceTypes = await _context.ServiceTypes.ToListAsync();

            // Mapping dữ liệu kết hợp
            var allData = regulationsWithGroups
                .GroupBy(x => new { x.ServiceGroupid, x.GroupName, x.Price })
                .Select(gr => new RegulationsDTO
                {
                    ServiceGroupid = gr.Key.ServiceGroupid,
                    GroupName = gr.Key.GroupName,
                    Price = gr.Key.Price,
                    ServiceTypes = allServiceTypes
                        .Where(st => st.ServiceGroupid == gr.Key.ServiceGroupid)
                        .Select(st => new ServiceTypeDTO
                        {
                            Id = st.Id,
                            ServiceTypeNames = st.ServiceTypename
                        })
                        .ToList()
                }).ToList();

            var totalRow = allData.Count;
            var pageCount = (int)Math.Ceiling(totalRow / (double)req.PageSize);

            // Phân trang dữ liệu đã xử lý
            var pagedResult = allData
                .OrderByDescending(x => x.GroupName)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToList();

            return new PagingResult<RegulationsDTO>
            {
                Results = pagedResult,
                CurrentPage = req.Page,
                RowCount = totalRow,
                PageSize = req.PageSize,
                PageCount = pageCount
            };
        }

        public async Task<PagingResult<EndowDTO>> GetAllEndow(GetListReq req)
        {
        var query = from c in _context.Endows
                        join a in _context.ServiceGroups
                        on c.ServiceGroupid equals a.ServiceGroupid
                    select new EndowDTO
                        {
                            Endowid = c.Endowid, 
                            ServiceGroupid = c.ServiceGroupid, 
                            Discount = c.Discount,
                            Startdate = c.Startdate,
                            Enddate = c.Enddate,
                            Duration = c.Duration,
                            Descriptionendow = c.Descriptionendow,
                            GroupName = a.GroupName,
                        };

            // Đếm tổng số dòng
            var totalRow = await query.CountAsync();

            var pagedResult = await query
                .OrderByDescending(x => x.GroupName)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

            var pageCount = (int)Math.Ceiling(totalRow / (double)req.PageSize);

            return new PagingResult<EndowDTO>
            {
                Results = pagedResult,
                CurrentPage = req.Page,
                RowCount = totalRow,
                PageSize = req.PageSize,
                PageCount = pageCount
            };
        }
        
        //thêm 1 nhóm dịch vụ mới ---có nhóm mới tên 1 cái 
        public string? InsertRegulation(RegulationsDTO regu, string id)
        {
            if (regu == null)
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

                    // Tìm mã ServiceGroupid lớn nhất hiện có
                    var lastId = _context.ServiceGroups
                        .OrderByDescending(s => s.ServiceGroupid)
                        .Select(s => s.ServiceGroupid)
                        .FirstOrDefault();

                    int nextNumber = 1;
                    if (!string.IsNullOrEmpty(lastId) && lastId.Length >= 7 && lastId.StartsWith("SER"))
                    {
                        string numberPart = lastId.Substring(3);
                        if (int.TryParse(numberPart, out int parsedNumber))
                        {
                            nextNumber = parsedNumber + 1;
                        }
                    }

                    string newServiceGroupId = $"SER{nextNumber.ToString("D4")}";

                    // Tạo nhóm dịch vụ mới
                    var newServiceGroup = new ServiceGroup
                    {
                        ServiceGroupid = newServiceGroupId,
                        GroupName = regu.GroupName,
                    };

                    foreach (var serviceGroup in regu.ServiceTypes)
                    {
                        var newServiceType = new ServiceType
                        {
                            ServiceGroupid = newServiceGroupId,
                            ServiceTypename = serviceGroup.ServiceTypeNames,
                        };
                        _context.ServiceTypes.Add(newServiceType);
                    }

                    var newRegulation = new Regulation
                    {
                        ServiceGroupid = newServiceGroupId,
                        Price = regu.Price,
                    };

                    _context.ServiceGroups.Add(newServiceGroup);
                    _context.Regulations.Add(newRegulation);
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

        //sửa tên nhóm dịch vụ
        public string? Update(RegulationsDTO regu, string id)
        {
            if (regu == null)
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

                    var existingID = _context.ServiceGroups.FirstOrDefault(c => c.ServiceGroupid == regu.ServiceGroupid);
                    if (existingID == null)
                    {
                        return "Mã dịch vụ không tồn tại";
                    }

                    // Cập nhật thông tin dịch vụ
                    existingID.GroupName = regu.GroupName;
                  
                    _context.ServiceGroups.Update(existingID);
                    _context.SaveChanges();

                    var existingRegu = _context.Regulations.FirstOrDefault(a => a.ServiceGroupid == regu.ServiceGroupid);
                    if (existingRegu == null)
                    {
                        return "Mã dịch vụ không tồn tại";
                    }
                    existingRegu.Price = regu.Price;
                   
                    _context.Regulations.Update(existingRegu);
                    _context.SaveChanges();

                    transaction.Commit();
                    return existingID.ServiceGroupid;
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

        //thêm 1 tên dịch vụ mới
        public string? InsertTypename(RegulationsDTO regu, string id)
        {
            if (regu == null)
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
                    var ServiceGroupid = _context.ServiceGroups.FirstOrDefault(s => s.ServiceGroupid == regu.ServiceGroupid);
                    if (ServiceGroupid == null)
                    {
                        return $"Mã dịch vụ = {ServiceGroupid} không tồn tại";
                    }
                   
                    foreach (var serviceGroup in regu.ServiceTypes)
                    {
                        var newServiceType = new ServiceType
                        {

                            ServiceGroupid = regu.ServiceGroupid,
                            ServiceTypename = serviceGroup.ServiceTypeNames,
                        };
                        _context.ServiceTypes.Add(newServiceType);
                    }
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
        
        //sửa tên 1 dịch vụ 
        public string? UpdateTypename(RegulationsDTO regu, string id)
        {
            if (regu == null || regu.ServiceTypes == null || !regu.ServiceTypes.Any())
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

                    foreach (var serviceTypeDto in regu.ServiceTypes)
                    {
                        var existingType = _context.ServiceTypes.FirstOrDefault(s => s.Id == serviceTypeDto.Id);
                        if (existingType == null)
                        {
                            return $"Không tìm thấy dịch vụ với Id = {serviceTypeDto.Id}";
                        }

                        // Cập nhật tên dịch vụ
                        existingType.ServiceTypename = serviceTypeDto.ServiceTypeNames;
                        _context.ServiceTypes.Update(existingType);
                    }

                    _context.SaveChanges();
                    transaction.Commit();

                    return regu.ServiceGroupid; // Trả về mã nhóm dịch vụ đã cập nhật
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

        //xóa tên 1 dịch vụ
        public string? DeleteTypename(RegulationsDTO regu, string id)
        {
            if (regu == null || regu.ServiceTypes == null || !regu.ServiceTypes.Any())
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

                    foreach (var serviceTypeDto in regu.ServiceTypes)
                    {
                        var existingType = _context.ServiceTypes.FirstOrDefault(s =>
                            s.Id == serviceTypeDto.Id &&
                            s.ServiceTypename == serviceTypeDto.ServiceTypeNames);

                        if (existingType == null)
                        {
                            return $"Không tìm thấy dịch vụ với Id = {serviceTypeDto.Id} và tên = {serviceTypeDto.ServiceTypeNames}";
                        }

                        _context.ServiceTypes.Remove(existingType);
                    }

                    _context.SaveChanges();
                    transaction.Commit();

                    return null ; // Trả về mã nhóm dịch vụ đã xóa
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

        //Thêm ưu đãi mới 
        public string? InsertEndow(EndowDTO endow, string id)
        {
            if (endow == null)
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

                    // Tìm mã ServiceGroupid lớn nhất hiện có
                    var lastId = _context.Endows
                        .OrderByDescending(s => s.Endowid)
                        .Select(s => s.Endowid)
                        .FirstOrDefault();

                    int nextNumber = 1;
                    if (!string.IsNullOrEmpty(lastId) && lastId.Length >= 10 && lastId.StartsWith("ENDOW"))
                    {
                        string numberPart = lastId.Substring(5);
                        if (int.TryParse(numberPart, out int parsedNumber))
                        {
                            nextNumber = parsedNumber + 1;
                        }
                    }

                    string newEndow = $"ENDOW{nextNumber.ToString("D5")}";

                    var newEndowinsert = new Endow
                    {
                        Endowid = newEndow, 
                        ServiceGroupid = endow.ServiceGroupid,
                        Discount = endow.Discount,
                        Startdate = endow.Startdate,
                        Enddate = endow.Enddate,
                        Duration = endow.Duration,
                        Descriptionendow = endow.Descriptionendow,
                    };
                    _context.Endows.Add(newEndowinsert);
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
        
        //Cập nhật ưu đãi cũ
        public string? UpdateEndow(EndowDTO endow, string id)
        {
            if (endow == null)
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

                    var existingID = _context.Endows.FirstOrDefault(c => c.Endowid == endow.Endowid);
                    if (existingID == null)
                    {
                        return "Mã ưu đãi không tồn tại";
                    }

                    // Cập nhật thông tin dịch vụ
                    //var ServiceGroupid = _context.ServiceGroups.FirstOrDefault(c => c.GroupName == endow.GroupName);
                    //existingID.ServiceGroupid = ServiceGroupid.ServiceGroupid;
                    existingID.Discount = endow.Discount;
                    existingID.Duration = endow.Duration;
                    existingID.Descriptionendow = endow.Descriptionendow;

                    _context.Endows.Update(existingID);
                    _context.SaveChanges();

                    
                    transaction.Commit();
                    return existingID.Endowid;
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

        public async Task<List<ServiceGroup>> GetListServiceID()
        {
            // Lấy danh sách các nhóm dịch vụ cần thiết
            var regulationsWithGroups = await _context.ServiceGroups.ToListAsync();
            return regulationsWithGroups;
        }

    }
}

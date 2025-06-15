using Microsoft.EntityFrameworkCore;
using WebApi.DTO;
using WebApi.Models;

namespace WebApi.Service.Introduce
{
    public class ServiceGuest
    {
        private readonly ManagementDbContext _context;
        public ServiceGuest(ManagementDbContext context)
        {
            _context = context;
        }

        public async Task<List<ServiceTypeDTO1>> GetAll()
        {
            var query = from st in _context.ServiceTypes
                        join r in _context.Regulations on st.ServiceGroupid equals r.ServiceGroupid
                        join g in _context.ServiceGroups on st.ServiceGroupid equals g.ServiceGroupid
                        select new ServiceTypeDTO1
                        {
                            Id = st.Id,
                            ServiceTypeNames = st.ServiceTypename,
                            Descriptionsr = st.DescriptionSr,
                            GroupName = g.GroupName,
                            Price = r.Price
                        };

            return await query.ToListAsync();
        }


        public async Task<PagingResult<ServiceTypeDTO1>> GetAllRegulations(GetListReq req)
        {
            // Join để lấy đầy đủ thông tin ServiceType + GroupName + Price từ Regulations
            var query = from st in _context.ServiceTypes
                        join r in _context.Regulations on st.ServiceGroupid equals r.ServiceGroupid
                        join g in _context.ServiceGroups on st.ServiceGroupid equals g.ServiceGroupid
                        select new ServiceTypeDTO1
                        {
                            Id = st.Id,
                            ServiceTypeNames = st.ServiceTypename,
                            Descriptionsr = st.DescriptionSr,
                            GroupName = g.GroupName,
                            Price = r.Price
                        };

            var totalRow = await query.CountAsync();

            var pagedData = await query
                .OrderByDescending(x => x.GroupName)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

            return new PagingResult<ServiceTypeDTO1>
            {
                Results = pagedData,
                CurrentPage = req.Page,
                PageSize = req.PageSize,
                RowCount = totalRow,
                PageCount = (int)Math.Ceiling(totalRow / (double)req.PageSize)
            };
        }

    }
}

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebApi.DTO;
using WebApi.Models;

namespace WebApi.Service.Admin
{
    public interface IAdminContactService
    {
        Task<(bool Success, GetListContactRes res)> GetListPaging(GetListContactPaging req);
        Task<bool> UpdateStatus(int id, int status);
        Task<(bool Success, ContactModelRes res)> GetById(int id);
    }
    public class AdminContactService : IAdminContactService
    {
        private readonly ManagementDbContext _context;
        public AdminContactService(ManagementDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, ContactModelRes res)> GetById(int id)
        {
            var data = await _context.Contacts.Where(item=> item.Id == id)
                .Select(item => new ContactModelRes()
                 {
                     CreatedDate = item.CreatedDate.Value,
                     Message = item.Message,
                     Email = item.Email,
                     Phone = item.Phone,
                     Name = item.Name,
                     Status = item.Status,
                     Subject = item.Subject,
                     Id = item.Id,
                 }).FirstOrDefaultAsync();

            if (data == null) {
                return (false, null);
            }
            return (true, data);
        }

        public async Task<(bool Success, GetListContactRes res)> GetListPaging(GetListContactPaging req)
        {
            req.CheckValue();

            var res = new GetListContactRes()
            {
                Data = new List<ContactModelRes>(),
                TotalRow = 0
            };

            IQueryable<Contact> query = _context.Contacts;

            if (req.Status >= 0)
            {
                query = query.Where(n => n.Status == req.Status);
            }

            if (!string.IsNullOrEmpty(req.KeyWord))
            {
                var keyword = req.KeyWord.Trim();
                query = query.Where(n => n.Name.Contains(keyword) ||
                                         (!string.IsNullOrEmpty(n.Message) && n.Message.Contains(keyword)) ||
                                         (!string.IsNullOrEmpty(n.Email) && n.Email.Contains(keyword)) ||
                                         n.Phone.Contains(keyword));
            }

            var pagedResult = await query
                .OrderByDescending(c => c.Id)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();


            var totalRow = await query.CountAsync();

            res.Data = pagedResult.Select(item => new ContactModelRes()
            {
                CreatedDate = item.CreatedDate.Value,
                Message = item.Message,
                Email = item.Email,
                Phone = item.Phone,
                Name = item.Name,
                Status = item.Status,
                Subject = item.Subject,
                Id = item.Id,
            }).ToList();

            res.TotalRow = totalRow;
            return (true, res);
        }

        public async Task<bool> UpdateStatus(int id, int status)
        {
            var data = await _context.Contacts.Where(item => item.Id == id).FirstOrDefaultAsync();
            if (data == null)
                return false;

            data.Status = status;
            _context.Contacts.Update(data);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}

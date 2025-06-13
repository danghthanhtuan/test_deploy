using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using WebApi.DTO;
using WebApi.Models;

namespace WebApi.Service.Admin
{
    public interface INotificationService
    {
        Task<(bool Success, GetListNotificationRes res)> GetListPaging(GetListNotificationPaging req);
        Task<(bool Success, CreateNotificationResponseDTO res)> CreateNotification(CreateNotificationDTO model);
        Task<bool> Update(int id);
    }
    public class NotificationService : INotificationService
    {
        private readonly ManagementDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        public NotificationService(ManagementDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<(bool Success, GetListNotificationRes res)> GetListPaging(GetListNotificationPaging req)
        {
            req.CheckValue();

            var res = new GetListNotificationRes()
            {
                Data = new List<NotificationModelRes>(),
                IsNextPage = false
            };

            var query = _context.Notifications;

            var countUnRead = await query.Where(item => item.IsRead == false).CountAsync();

            if (req.IsRead > 0)
            {
                var isRead = (req.IsRead == 1 ? true : false);
                query.Where(n => n.IsRead == isRead);
            }

            var pagedResult = await query
                .OrderByDescending(c => c.Id)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

            var isNextPage = await query
                .Skip(req.Page)
                .Take(1).AnyAsync();

            res.Data = pagedResult.Select(item => new NotificationModelRes()
            {
                Data = item.Data,
                Content = item.Content,
                CreatedAt = item.CreatedAt,
                Id = item.Id,
                IsRead = item.IsRead,
                ReferenceId = item.ReferenceId,
                Title = item.Title,
                Typenoti = item.Typenoti,
                UpdatedAt = item.UpdatedAt,
                UserId = item.UserId,
            }).ToList();

            res.IsNextPage = isNextPage;
            res.CountUnRead = countUnRead;
            return (true, res);
        }

        public async Task<(bool Success, CreateNotificationResponseDTO res)> CreateNotification(CreateNotificationDTO model)
        {
            var res = new CreateNotificationResponseDTO()
            {
                Id = 0,
                Message = "Đã có lỗi xảy ra, Vui lòng thử lại sau."
            };

            if (string.IsNullOrEmpty(model.Title) || string.IsNullOrEmpty(model.Content))
            {
                res.Message = "Dữ liệu đầu vào không hợp lệ.";
                return (false, res);
            }

            try
            {
                var noti = new Notification
                {
                    Data = model.Data,
                    Content = model.Content,
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    ReferenceId = model.ReferenceId,
                    Title = model.Title,
                    Typenoti = model.Type,
                    UserId = model.UserId,
                };

                _context.Notifications.Add(noti);
                await _context.SaveChangesAsync();

                res.Message = "Tạo thành công!";
                res.Id = noti.Id;

                await _hubContext.Clients.All.SendAsync("UserGuest", noti);

                return (true, res);
            }
            catch
            {
                return (false, res);
            }
        }

        public async Task<bool> Update(int id)
        {
            var data = await _context.Notifications.Where(item => item.Id == id).FirstOrDefaultAsync();
            if (data == null)
                return false;

            data.IsRead = true;
            data.UpdatedAt = DateTime.Now;
            _context.Notifications.Update(data);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}

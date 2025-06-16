using WebApi.DTO;
using WebApi.Enum;
using WebApi.Helper;
using WebApi.Models;
using WebApi.Service.Admin;

namespace WebApi.Service.Client
{
    public class ContactService
    {
        private readonly ManagementDbContext _context;
        private readonly INotificationService _notificationService;

        public ContactService(ManagementDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }


        public async Task<(bool Success, string Message)> CreateContact(CreateContactDTO model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Phone) || string.IsNullOrEmpty(model.Name))
            {
                return (false, "Vui lòng nhập đầy đủ thông tin.");
            }

            try
            {
                var contact = new Contact
                {
                    Email = model.Email,
                    Phone = model.Phone,
                    Name = model.Name,
                    Message = model.Message,
                    Subject = model.Subject,
                    CreatedDate = DateTime.Now,
                    Status = (int)ContactStatusEnum.New,
                };

                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();

                var dataNoti = new CreateNotificationDTO()
                {
                    Content = CommonHelper.GetContentNoti(NotificationTypeEnum.Contact, model.Name, model.Phone),
                    Data = "",
                    ReferenceId = contact.Id,
                    Title = CommonHelper.GetTitleNoti(NotificationTypeEnum.Contact),
                    Type = (int)NotificationTypeEnum.Contact,
                    UserId = null
                };

                await _notificationService.CreateNotification(dataNoti);

                return (true, "Tạo thành công!");
            }
            catch
            {
                return (false, "Tạo thất bại, vui lòng thử lại.");
            }
        }
    }
}

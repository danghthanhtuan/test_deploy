using WebApi.Enum;

namespace WebApi.Helper
{
    public class CommonHelper
    {
        public static string GetTitleNoti(NotificationTypeEnum notificationType)
        {
            switch (notificationType)
            {
                case NotificationTypeEnum.Contact:
                    return "Người dùng liên hệ";

                case NotificationTypeEnum.SignContract:
                    return "Người dùng ký hợp đồng";
            }

            return string.Empty;
        }

        public static string GetContentNoti(NotificationTypeEnum notificationType, string userName, string phone)
        {
            switch (notificationType)
            {
                case NotificationTypeEnum.Contact:
                    return $"Người dùng {userName} số điện thoại {phone} vừa gửi một thông tin liên hệ, Vui lòng kiểm tra thông tin!";

                case NotificationTypeEnum.SignContract:
                    return $"Người dùng {userName} số điện thoại {phone} vừa ký hợp đồng, Vui lòng kiểm tra thông tin!";
            }

            return string.Empty;
        }
    }
}

namespace WebApp.DTO
{
    public class RegisterDTO
    {
        public string StaffName { get; set; } = null!;
        public string StaffPhone { get; set; } = null!;
        public string PassWordAd { get; set; } = null!;
        public string Department { get; set; } = null!;

    }
    public class APIResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
    }
    public class LoginRequest
    {
        public string UserName { get; set; } = null!;
        public string PassWord { get; set; } = null!;
    }

    public class LoginRequestClient
    {
        public string UserName { get; set; } = null!;
        public string PassWord { get; set; } = null!;
        public string Contractnumber { get; set; } = null!;
    }

    public class Accountlogin
    {
        public string Customerid { get; set; } = null!;

        public string Rootaccount { get; set; } = null!;

        public string Rootname { get; set; } = null!;

        public string Rphonenumber { get; set; } = null!;

        public string Contractnumber { get; set; } = null!;

    }

    public class RegisterclientDTO
    {
        public string companyID { get; set; } = null!;
        public string rootPhone { get; set; } = null!;
        public string PassWord { get; set; } = null!;
    }

    public class LoginResponseModel1
    {
        public string? SDT { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; } // Thêm Refresh Token
        public int ExpiresIn { get; set; }
        public DateTime RefreshTokenExpiry { get; set; } // Thời gian hết hạn của Refresh Token
    }
    public class RefreshTokenRequest
    {
        public string? SDT { get; set; } // Số điện thoại để xác định user
        public string? RefreshToken { get; set; } // Refresh Token hiện tại của user
    }
}

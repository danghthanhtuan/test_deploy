namespace WebApi.Content
{
    public class SendEmailRegister
    {
        public string SendEmail_Register(int otp, string hoTen)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Mã OTP xác thực</title>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #f9f9f9;
                            text-align: center;
                            padding: 20px;
                        }}
                        .container {{
                            background: white;
                            padding: 20px;
                            border-radius: 8px;
                            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                            display: inline-block;
                        }}
                        .otp-code {{
                            font-size: 28px;
                            font-weight: bold;
                            color: #007bff;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>Hệ thống quản lý doanh nghiệp</h2>
                        <p>Xin chào {hoTen}</p>
                        <p>Mã OTP của bạn là:</p>
                        <p class='otp-code'>{otp}</p>
                        <p>Mã này có hiệu lực trong 5 phút. Vui lòng không chia sẻ với ai.</p>
                        <p>&copy; 2025 Hệ thống quản lý doanh nghiệp.</p>
                    </div>
                </body>
                </html>";
        }

        public string SendEmail_pass(string pass, string email)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Mã OTP xác thực</title>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #f9f9f9;
                            text-align: center;
                            padding: 20px;
                        }}
                        .container {{
                            background: white;
                            padding: 20px;
                            border-radius: 8px;
                            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                            display: inline-block;
                        }}
                        .otp-code {{
                            font-size: 28px;
                            font-weight: bold;
                            color: #007bff;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>Hệ thống quản lý doanh nghiệp</h2>
                        <p>Xin chào {email}</p>
                        <p>Mật khẩu mới của bạn là:</p>
                        <p class='otp-code'>{pass}</p>
                        <p>&copy; 2025 Hệ thống quản lý doanh nghiệp.</p>
                    </div>
                </body>
                </html>";
        }

    }
}

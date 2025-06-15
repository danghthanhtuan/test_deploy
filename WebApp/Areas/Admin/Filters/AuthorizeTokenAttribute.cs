using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WebApp.Configs;

public class AuthorizeTokenAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;
        var token = httpContext.Request.Cookies["AuthToken"];

        // Lấy cấu hình từ DI container thông qua RequestServices
        var apiConfigs = httpContext.RequestServices
            .GetService(typeof(IOptions<ApiConfigs>)) as IOptions<ApiConfigs>;

        if (apiConfigs == null)
        {
            context.Result = new ContentResult
            {
                Content = "Cấu hình API chưa được khởi tạo.",
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
            return;
        }

        var baseApiUrl = apiConfigs.Value.BaseApiUrl;

        // Nếu không có token và đang truy cập trang Login -> Cho phép đi tiếp
        if (string.IsNullOrEmpty(token) && httpContext.Request.Path.Value.Contains("/admin/LoginAdmin/Login", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Nếu có token nhưng hết hạn -> Xóa token và chuyển hướng đến trang Login
        if (!string.IsNullOrEmpty(token) && !ValidateToken(token, baseApiUrl).GetAwaiter().GetResult())
        {
            context.HttpContext.Response.Cookies.Delete("AuthToken");
            context.Result = new RedirectToActionResult("Login", "LoginAdmin", null);
            return;
        }

        // Nếu đã đăng nhập mà vào trang Login, thì chuyển hướng về HomeAdmin
        if (!string.IsNullOrEmpty(token) && httpContext.Request.Path.Value.Contains("/admin/LoginAdmin/Login", StringComparison.OrdinalIgnoreCase))
        {
            context.HttpContext.Response.Redirect("/admin/homeadmin/Index");
            context.Result = new EmptyResult();
        }
    }

    private async Task<bool> ValidateToken(string token, string baseApiUrl)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", token);

            var response = await client.GetAsync($"{baseApiUrl}/admin/home/ValidateToken/validate-token");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return false;
            }

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

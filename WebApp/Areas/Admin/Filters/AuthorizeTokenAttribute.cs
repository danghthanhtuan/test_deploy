using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.IO;
using System.Net;

public class AuthorizeTokenAttribute : Attribute, IAuthorizationFilter
{
    private readonly Uri baseAddress = new Uri("https://localhost:7190/api/admin");
    private readonly HttpClient _client;

    public AuthorizeTokenAttribute()
    {
        _client = new HttpClient();
        _client.BaseAddress = baseAddress;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;
        var token = httpContext.Request.Cookies["AuthToken"];

        // Nếu không có token và đang truy cập trang Login -> Cho phép đi tiếp
        if (string.IsNullOrEmpty(token) && httpContext.Request.Path.Value.Contains("/admin/LoginAdmin/Login"))
        {
            return;
        }

        // Nếu có token nhưng hết hạn -> Xóa token và chuyển hướng đến trang Login
        if (!string.IsNullOrEmpty(token) && !ValidateToken(token).GetAwaiter().GetResult())
        {
            context.HttpContext.Response.Cookies.Delete("AuthToken");
            context.Result = new RedirectToActionResult("Login", "LoginAdmin", null); // Chuyển hướng về trang login
            return;
        }

        // Nếu đã đăng nhập mà vào trang Login, thì chuyển hướng về HomeAdmin
        if (!string.IsNullOrEmpty(token) && httpContext.Request.Path.Value.Contains("/admin/LoginAdmin/Login"))
        {
            context.HttpContext.Response.Redirect("/admin/homeadmin/Index");
            context.Result = new EmptyResult();
        }
    }

    private async Task<bool> ValidateToken(string token)
    {
        if (_client == null)
        {
            throw new Exception("_client chưa được khởi tạo.");
        }

        try
        {
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", token);

            HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + "/home/ValidateToken/validate-token");

            if (response.StatusCode == HttpStatusCode.Unauthorized) // Token hết hạn
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


    //private async Task<bool> ValidateToken(string token)
    //{
    //    _client.DefaultRequestHeaders.Clear();
    //    _client.DefaultRequestHeaders.Add("Authorization", token);
    //    HttpResponseMessage response = await _client.GetAsync(_client.BaseAddress + "/home/ValidateToken/validate-token");
    //    string dataJson = await response.Content.ReadAsStringAsync();

    //    if (response.IsSuccessStatusCode)
    //    {
    //        return true;
    //    }
    //    return false;
    //}


}
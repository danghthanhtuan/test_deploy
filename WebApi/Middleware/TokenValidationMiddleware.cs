using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;

    public TokenValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.Value.ToLower();

        // Bỏ qua middleware nếu request đến trang đăng nhập
        if (path.Contains("/admin/LoginAdmin/Login"))
        {
            await _next(context);
            return;
        }

        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (string.IsNullOrEmpty(token))
        {
            context.Response.Redirect("/admin/LoginAdmin/Login");
            return;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null || jwtToken.ValidTo < DateTime.UtcNow) // Token hết hạn
            {
                context.Response.Redirect("/admin/LoginAdmin/Login");
                return;
            }
        }
        catch
        {
            context.Response.Redirect("/admin/LoginAdmin/Login");
            return;
        }

        await _next(context);
    }
}

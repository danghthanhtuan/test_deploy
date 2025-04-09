using Azure.Core;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Text.Json;
using WebApi.Models;
using WebApi.DTO;
using WebApi.Service.Admin;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace WebApi.Controllers
{
    [Route("api/admin/[controller]/[action]")]
    [ApiController]

    public class HomeController : Controller
    {
        private readonly ManagementDbContext _context;
        private readonly HomeService _homeService;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;

        public HomeController(ManagementDbContext context, HomeService homeService, JwtService jwtService, IConfiguration configuration)
        {
            _context = context; // Gán biến context
            _homeService = homeService;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequesta model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.PassWord))
                {
                    return BadRequest(new { success = false, message = "Tài khoản hoặc mật khẩu không được để trống." });
                }

                Staff nhanViens = _homeService.LoginAsync(model.UserName, model.PassWord);

                if (nhanViens != null)
                {
                    var token = await _jwtService.CreateTokenAdmin(nhanViens.Staffname, nhanViens.Department);

                    return Ok(new APIResponse<Staff>()
                    {
                        Success = true,
                        Message = token!.AccessToken!,
                        Data = nhanViens
                    });
                }
                else
                {
                    string checkLogin = _homeService.CheckLogin(model.UserName, model.PassWord);

                    return Ok(new APIResponse<object>()
                    {
                        Success = false,
                        Message = checkLogin,
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("validate-token")]
        public IActionResult ValidateToken()
        {
            try
            {
                var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new { message = "Token is required" });
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["JwtConfig:Key"]!);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JwtConfig:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JwtConfig:Audience"],
                    ValidateLifetime = true, // Kiểm tra token hết hạn
                    ClockSkew = TimeSpan.Zero // Không cho phép thời gian trễ
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Nếu token hết hạn, trả về 401
                if (validatedToken.ValidTo < DateTime.UtcNow)
                {
                    return Unauthorized(new { message = "Token has expired" });
                }

                return Ok(); // Token hợp lệ
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized(new { message = "Token has expired" });
            }
            catch (SecurityTokenException)
            {
                return Unauthorized(new { message = "Invalid token" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        //[HttpGet("validate-token")]
        //public IActionResult ValidateToken()
        //{
        //    try
        //    {
        //        var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        //        if (string.IsNullOrEmpty(token))
        //        {
        //            return Unauthorized(new { message = "Token is required" });
        //        }

        //        var tokenHandler = new JwtSecurityTokenHandler();
        //        var key = Encoding.UTF8.GetBytes(_configuration["JwtConfig:Key"]!);

        //        var validationParameters = new TokenValidationParameters
        //        {
        //            ValidateIssuerSigningKey = true,
        //            IssuerSigningKey = new SymmetricSecurityKey(key),
        //            ValidateIssuer = true,
        //            ValidIssuer = _configuration["JwtConfig:Issuer"],
        //            ValidateAudience = true,
        //            ValidAudience = _configuration["JwtConfig:Audience"],
        //            ValidateLifetime = true, // Kiểm tra token hết hạn
        //            ClockSkew = TimeSpan.Zero // Không cho phép thời gian trễ
        //        };

        //        var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

        //        // Nếu token hết hạn, trả về 401
        //        if (validatedToken.ValidTo < DateTime.UtcNow)
        //        {
        //            return Unauthorized(new { message = "Token has expired" });
        //        }

        //        return Ok(); // Token hợp lệ
        //    }
        //    catch (SecurityTokenExpiredException)
        //    {
        //        return Unauthorized(new { message = "Token has expired" });
        //    }
        //    catch (SecurityTokenException)
        //    {
        //        return Unauthorized(new { message = "Invalid token" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}

        //[HttpGet]
        //public IActionResult CheckToken()
        //{
        //    var authHeader = Request.Headers["Authorization"].ToString();
        //    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        //    {
        //        return Unauthorized("Missing token");
        //    }

        //    var token = authHeader.Substring(7);
        //    var handler = new JwtSecurityTokenHandler();

        //    try
        //    {
        //        var jwtToken = handler.ReadJwtToken(token);
        //        if (jwtToken.ValidTo < DateTime.UtcNow)
        //        {
        //            return Unauthorized("Token expired");
        //        }
        //        return Ok("Token valid");
        //    }
        //    catch
        //    {
        //        return Unauthorized("Invalid token");
        //    }
        //}

    }
}

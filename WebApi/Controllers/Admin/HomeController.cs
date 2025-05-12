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
using WebApi.Service.Client;

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

        [HttpPost]
        public async Task<IActionResult> SendEmail_OTP([FromBody] JsonElement infoUser)
        {
            try
            {
                if (infoUser.ValueKind == JsonValueKind.Object)
                {
                    string phoneNumber = infoUser.GetProperty("phoneNumber").GetString();
                    string userEmail = infoUser.GetProperty("userEmail").GetString();

                    var result = await _homeService.SendEmail_OTP(phoneNumber, userEmail);

                    return Ok(new APIResponse<object>
                    {
                        Success = result == "Ok",
                        Message = result,
                        Data = null
                    });
                }

                return BadRequest(new APIResponse<object>
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ.",
                    Data = null
                });
            }
            catch (Exception e)
            {
                return BadRequest(new APIResponse<object>
                {
                    Success = false,
                    Message = e.Message,
                    Data = null
                });
            }
        }


        [HttpGet("{phoneNumber}/{otp}")]
        public async Task<IActionResult> CheckEmail_Register(string phoneNumber, string otp)
        {
            try
            {
                var result = await _homeService.CheckEmail_Register(phoneNumber, otp);

                if (result == "OTP hợp lệ!")
                {
                    return Ok(new APIResponse<object> { Success = true, Message = result });
                }

                return Ok(new APIResponse<object> { Success = false, Message = result });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpPost]
        public async Task<IActionResult> UpdatePassword([FromBody] LoginRequesta model)
        {
            var result = await _homeService.UpdatePassword(model);
            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }
    }
}

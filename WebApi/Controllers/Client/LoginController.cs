﻿using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApi.DTO;
using WebApi.Models;
using WebApi.Service.Admin;
using WebApi.Service.Client;

namespace WebApi.Controllers.Client
{
    [Route("api/client/[controller]/[action]")]
    [ApiController]
    public class LoginController : Controller
    {
        private readonly ManagementDbContext _context;
        private readonly JwtService _jwtService;
        private readonly LoginService _loginService;

        public LoginController(ManagementDbContext context, JwtService jwtService, LoginService loginService)
        {
            _context = context; // Gán biến context
            _jwtService = jwtService;
            _loginService = loginService;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequestClient model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.PassWord))
                {
                    return BadRequest(new { success = false, message = "Tài khoản hoặc mật khẩu không được để trống." });
                }

                Accountlogin clients = _loginService.LoginAsync(model.UserName, model.PassWord, model.Contractnumber);

                if (clients != null)
                {
                    var token = await _jwtService.CreateTokenUser(clients.Rootname, clients.Contractnumber);

                    return Ok(new APIResponse<Accountlogin>()
                    {
                        Success = true,
                        Message = token!.AccessToken!,
                        Data = clients
                    });
                }
                else
                {
                    string checkLogin = _loginService.CheckLogin(model.UserName, model.PassWord, model.Contractnumber);

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

        [HttpPost]
        public async Task<IActionResult> SendEmail_OTP([FromBody] JsonElement infoUser)
        {
            try
            {
                if (infoUser.ValueKind == JsonValueKind.Object)
                {
                    string phoneNumber = infoUser.GetProperty("phoneNumber").GetString();
                    string userEmail = infoUser.GetProperty("userEmail").GetString();

                    var result = await _loginService.SendEmail_OTP(phoneNumber, userEmail);

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
                var result = await _loginService.CheckEmail_Register(phoneNumber, otp);

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
            var result = await _loginService.UpdatePassword(model);
            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }
    }
}

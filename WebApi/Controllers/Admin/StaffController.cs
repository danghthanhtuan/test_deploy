using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTO;
using WebApi.Models;
using WebApi.Service.Admin;

namespace WebApi.Controllers.Admin
{
    [Route("api/admin/[controller]/[action]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly StaffService _staffService;
        private readonly JwtService _jwtService;

        public StaffController(StaffService StaffService, JwtService jwtService)
        {
            _staffService = StaffService;
            _jwtService = jwtService;
        }
        [HttpGet]
        public IActionResult GetAllNhanVien()
        {
            try
            {
                List<Staff> nhanViens = _staffService.GetAllNhanVien();

                if (nhanViens != null)
                {
                    return Ok(new APIResponse<List<Staff>>()
                    {
                        Success = true,
                        Message = "Lấy dữ liệu thành công",
                        Data = nhanViens
                    });
                }
                else
                {
                    return Ok(new APIResponse<object>()
                    {
                        Success = false,
                        Message = "Không tìm thấy dữ liệu trong database",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            try
            {
                StaffDTO nhanViens = _staffService.GetById(id);

                if (nhanViens != null)
                {
                    return Ok(new APIResponse<StaffDTO>()
                    {
                        Success = true,
                        Message = "Lấy dữ liệu nhân viên thành công",
                        Data = nhanViens
                    });
                }
                else
                {
                    return Ok(new APIResponse<object>()
                    {
                        Success = false,
                        Message = "Không có dữ liệu của nhân viên",
                        Data = null
                    });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public async Task<IActionResult> ThemNhanVien([FromBody] StaffDTO obj)
        {
            try
            {
                if (await _staffService.ThemNhanVien(obj))
                {
                    return Ok(new APIResponse<object>()
                    {
                        Success = true,
                        Message = "Thêm dữ liệu thành công",
                        Data = null
                    });
                }
                else
                {
                    return Ok(new APIResponse<object>()
                    {
                        Success = false,
                        Message = "Số điện thoại đã tồn tại",
                        Data = null
                    });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public IActionResult UpdateThongTinNhanVien([FromBody] StaffDTO obj)
        {
            try
            {
                if (_staffService.UpdateThongTinNhanVien(obj))
                {
                    return Ok(new APIResponse<object>()
                    {
                        Success = true,
                        Message = "Cập nhật dữ liệu thành công",
                        Data = null
                    });
                }
                else
                {
                    return Ok(new APIResponse<object>()
                    {
                        Success = false,
                        Message = "Số điện thoại hoặc tên đã tồn tại",
                        Data = null
                    });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

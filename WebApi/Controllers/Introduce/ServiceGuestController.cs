using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTO;
using WebApi.Models;
using WebApi.Service.Admin;
using WebApi.Service.Introduce;

namespace WebApi.Controllers.Introduce
{
    [Route("api/introduce/[controller]/[action]")]
    [ApiController]
    public class ServiceGuestController : ControllerBase
    {
        private readonly ManagementDbContext _context;
        private readonly IMapper _mapper;
        private readonly ServiceGuest _ServiceGuest;

        public ServiceGuestController(IMapper mapper, ManagementDbContext context, ServiceGuest ServiceGuest)
        {
            _mapper = mapper;
            _context = context;
            _ServiceGuest = ServiceGuest;
        }

        [HttpPost]
        public async Task<ActionResult<List<ServiceTypeDTO1>>> GetAll()
        {
            var regu = await _ServiceGuest.GetAll();
            return Ok(regu);
        }


        [HttpPost]
        public async Task<ActionResult<ServiceTypeDTO1>> GetAllRegulations([FromBody] GetListReq req)
        {
            var regu = await _ServiceGuest.GetAllRegulations(req);
            return Ok(regu);
        }
    }
}

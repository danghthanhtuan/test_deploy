using AutoMapper;
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
    public class ContractController : ControllerBase
    {
        private readonly ManagementDbContext _context;
        private readonly IMapper _mapper;
        private readonly RequestService _requestService;
        private readonly AccountService _accountService;
        public ContractController(IMapper mapper, ManagementDbContext context, RequestService requestService, AccountService accountService)
        {
            _mapper = mapper;
            _requestService = requestService;
            _context = context;
            _accountService = accountService;
        }
        
        [HttpPost]
        public async Task<ActionResult<CompanyAccountDTO>> GetAllCompany([FromBody] GetListCompanyPaging req)
        {
            var company = await _accountService.GetAllCompany(req);
            return Ok(company);
        }

    }
}

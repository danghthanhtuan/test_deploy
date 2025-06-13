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
    public class TransactionController : ControllerBase
    {
        private readonly ManagementDbContext _context;
        private readonly TransactionService _tran;
        public TransactionController(ManagementDbContext context, TransactionService tran)
        {
            _context = context;
            _tran = tran;
        }

        [Authorize(Roles = "Admin,HanhChinh,Director")]
        [HttpPost]
        public async Task<ActionResult<PaymentTransactionDTO>> GetAllCompany([FromBody] GetListTransactionPaging req)
        {
            var tran = await _tran.GetAllCompany(req);
            return Ok(tran);
        }
    }
}

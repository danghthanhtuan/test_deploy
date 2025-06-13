using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.X9;
using System.Transactions;
using WebApi.DTO;
using WebApi.Models;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace WebApi.Service.Admin
{
    public class TransactionService
    {
        private readonly ManagementDbContext _context;
        public TransactionService(ManagementDbContext context)
        {
            _context = context;
        }

        public async Task<PagingResult<PaymentTransactionDTO>> GetAllCompany(GetListTransactionPaging req)
        {
            var query = from c in _context.Payments
                        join a in _context.PaymentTransactions on c.Id equals a.PaymentId
                        join b in _context.Contracts on c.Contractnumber equals b.Contractnumber
                        join h in _context.ContractFiles on b.Contractnumber equals h.Contractnumber
                        where ( b.Constatus ==5 || b.Constatus ==6 ) &&( h.FileStatus ==5 || h.FileStatus == 6)
                        group new { c, a, b, h } by new
                        {
                            c.Contractnumber,
                            c.Paymentstatus,
                            a.Amount,
                            a.TransactionCode,
                            a.BankCode,
                            a.BankTransactionCode,
                            a.CardType,
                            a.OrderInfo,
                            a.PaymentDate,
                            a.ResponseCode,
                            a.TmnCode,
                            a.PaymentMethod,
                            a.PaymentResult,
                            a.Email,
                            h.FilePath,
                            h.ConfileName,
                        } into g
                        select new PaymentTransactionDTO
                        {
                            Contractnumber = g.Key.Contractnumber,
                            Amount = g.Key.Amount ?? 0,
                            Paymentstatus = g.Key.Paymentstatus,
                            TransactionCode = g.Key.TransactionCode,
                            BankCode = g.Key.BankCode,
                            BankTransactionCode = g.Key.BankTransactionCode,
                            CardType = g.Key.CardType,
                            OrderInfo = g.Key.OrderInfo,
                            PaymentDate = g.Key.PaymentDate,
                            ResponseCode = g.Key.ResponseCode,
                            TmnCode = g.Key.TmnCode,
                            PaymentMethod = g.Key.PaymentMethod,
                            PaymentResult = g.Key.PaymentResult,
                            Email = g.Key.Email,
                            ConfileName = g.Key.ConfileName,
                            FilePath = g.Key.FilePath,
                        };

            var keyword = req.Keyword.ToLower();
                query = query.Where(c =>
                    (c.Contractnumber != null && c.Contractnumber.ToLower().Contains(keyword)) ||
                    (c.BankTransactionCode != null && c.BankTransactionCode.ToLower().Contains(keyword)) ||
                    (c.Email != null && c.Email.ToLower().Contains(keyword)));


            var endDate = req.End.Date.AddDays(1).AddTicks(-1); // 23:59:59.9999999

            if (req.End != default(DateTime))
            {
                if (req.Start != default(DateTime) && req.Start != null)
                {
                    query = query.Where(c => c.PaymentDate >= req.Start && c.PaymentDate <= endDate);
                }
                else
                {
                    query = query.Where(c => c.PaymentDate <= endDate);
                }
            }


            // Phân trang
            var totalRow = await query.CountAsync();
            var pagedResult = await query
                .OrderByDescending(c => c.PaymentDate)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

                var pageCount = (int)Math.Ceiling(totalRow / (double)req.PageSize);

            return new PagingResult<PaymentTransactionDTO>
            {
                Results = pagedResult,
                CurrentPage = req.Page,
                RowCount = totalRow,
                PageSize = req.PageSize,
                PageCount = pageCount
            };
        }

    }
}

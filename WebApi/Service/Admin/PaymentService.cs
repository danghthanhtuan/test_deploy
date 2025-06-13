using iText.Commons.Actions.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Transactions;
using WebApi.DTO;
using WebApi.Models;

namespace WebApi.Service.Admin
{
    public class PaymentService : IPaymentService
    {
        private readonly ManagementDbContext _context;

        public PaymentService(ManagementDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentTransaction> CreatePaymentAsync(PaymentCreateRequest request)
        {
            var payment = _context.Payments.FirstOrDefault(p =>
                p.Contractnumber == request.MaHopDong && p.Paymentstatus == false);
            if (payment == null)
                throw new Exception("Không tìm thấy hợp đồng hợp lệ.");

            var transactionCode = $"ORD{DateTime.Now:yyyyMMddHHmmssfff}"; // Mã duy nhất cho VNPAY

            var paymenttran = new PaymentTransaction
            {
                Amount = request.SoTien,
                PaymentId = payment.Id,
                TransactionCode = transactionCode,
                PaymentResult = 2 // DangXuLy

            };

            _context.PaymentTransactions.Add(paymenttran);
            await _context.SaveChangesAsync();

            return paymenttran;
        }

        public bool ThanhToan(PaymentTr request)
        {
            var paymenttransaction = _context.PaymentTransactions
                .FirstOrDefault(p => p.TransactionCode == request.ID);
            if (paymenttransaction == null)
                return false;

            var payment = _context.Payments.FirstOrDefault(p => p.Id == paymenttransaction.PaymentId);

            var contract = _context.Contracts.FirstOrDefault(c => c.Contractnumber == payment.Contractnumber);
            var account = _context.Accounts.FirstOrDefault(a => a.Customerid == contract.Customerid);

            if (payment == null)
                return false;

            decimal.TryParse(request.SoTien, out decimal amount);
            DateTime.TryParseExact(request.NgayThanhToan, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime paymentDate);

            paymenttransaction.TransactionCode = request.MaGiaoDich; // Có thể bỏ nếu không muốn ghi đè
            paymenttransaction.Amount = amount / 100;
            paymenttransaction.BankCode = request.MaNganHang;
            paymenttransaction.BankTransactionCode = request.MaGiaoDichNganHang;
            paymenttransaction.CardType = request.LoaiThe;
            paymenttransaction.OrderInfo = request.NoiDung;
            paymenttransaction.PaymentDate = paymentDate;
            paymenttransaction.ResponseCode = request.MaPhanHoi;
            paymenttransaction.TmnCode = request.MaWebsite;
            paymenttransaction.PaymentMethod = request.PhuongThuc;
            paymenttransaction.PaymentResult = (request.TinhTrang == "Thanh cong") ? 1 : 0;
            paymenttransaction.Email = account?.Rootaccount;

            _context.PaymentTransactions.Update(paymenttransaction);

            if (request.TinhTrang == "Thanh cong")
            {
                payment.Paymentstatus = true;
                _context.Payments.Update(payment);

                if (contract != null)
                {
                    contract.Constatus = 5;
                    _context.Contracts.Update(contract);

                    _context.ContractStatusHistories.Add(new ContractStatusHistory
                    {
                        Contractnumber = payment.Contractnumber,
                        OldStatus = 4,
                        NewStatus = 5,
                        ChangedAt = DateTime.Now,
                        ChangedBy = account?.Rootaccount
                    });

                    var oldContractFile = _context.ContractFiles
                        .FirstOrDefault(f => f.Contractnumber == payment.Contractnumber && f.FileStatus == 3);
                    if (oldContractFile != null)
                    {
                        _context.ContractFiles.Add(new ContractFile
                        {
                            Contractnumber = payment.Contractnumber,
                            ConfileName = oldContractFile.ConfileName,
                            FilePath = oldContractFile.FilePath,
                            UploadedAt = DateTime.Now,
                            FileStatus = 5
                        });
                    }
                }
            }

            _context.SaveChanges();
            return true;
        }



        public async Task<CompanyContractDTOs?> GetByContractNumberAsync(string contractNumber)
        {
            contractNumber = contractNumber?.Trim();
            var result = await (
                from c in _context.Companies
                join a in _context.Accounts on c.Customerid equals a.Customerid
                join h in _context.Contracts on c.Customerid equals h.Customerid
                join q in _context.ServiceTypes on h.ServiceTypeid equals q.Id
                join f in _context.ContractFiles on h.Contractnumber equals f.Contractnumber into fileJoin
                from file in fileJoin.DefaultIfEmpty()
                join p in _context.Payments on h.Contractnumber equals p.Contractnumber into paymentJoin
                from payment in paymentJoin.DefaultIfEmpty()
                where h.Contractnumber == contractNumber
                select new CompanyContractDTOs
                {
                    CustomerId = c.Customerid,
                    CompanyName = c.Companyname,
                    TaxCode = c.Taxcode,
                    CompanyAccount = c.Companyaccount,
                    AccountIssuedDate = c.Accountissueddate,
                    CPhoneNumber = c.Cphonenumber,
                    CAddress = c.Caddress,
                    RootAccount = a.Rootaccount,
                    RootName = a.Rootname,
                    RPhoneNumber = a.Rphonenumber,
                    DateOfBirth = a.Dateofbirth,
                    Gender = a.Gender,
                    ContractNumber = h.Contractnumber,
                    Startdate = h.Startdate,
                    Enddate = h.Enddate,
                    CustomerType = h.Customertype,
                    ServiceType = q.ServiceTypename,
                    ConfileName = file.ConfileName,
                    FilePath = file.FilePath,
                    Amount = payment.Amount,
                    Constatus = h.Constatus
                }
            ).FirstOrDefaultAsync();

            return result;
        }
    }
}

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
            var paymenttran = new PaymentTransaction
            {
                Amount = request.SoTien,
                PaymentId = payment.Id
            };

            _context.PaymentTransactions.Add(paymenttran);
            await _context.SaveChangesAsync();

            return paymenttran;
        }

        public bool ThanhToan(string ID, string maGiaoDich, string phuongThuc, string tinhTrang)
        {
            if (!int.TryParse(ID, out int idInt))
            {
                return false;
            }

            var paymenttransaction = _context.PaymentTransactions
                .FirstOrDefault(p => p.Id == idInt);

            var payment = _context.Payments.FirstOrDefault(p => p.Id == paymenttransaction.PaymentId);
            if (payment == null) return false;

            bool PaymentResultint;
            if (tinhTrang == "Thanh cong")
            {
                PaymentResultint = true;
            }
            else
            {
                PaymentResultint = false;
            }

            paymenttransaction.TransactionCode = maGiaoDich;
            paymenttransaction.PaymentDate = DateTime.Now;
            paymenttransaction.PaymentMethod = phuongThuc;
            paymenttransaction.PaymentResult = PaymentResultint;

            _context.PaymentTransactions.Update(paymenttransaction);

            if (tinhTrang == "Thanh cong")
            {
                payment.Paymentstatus = true;
                _context.Payments.Update(payment);

                var contract = _context.Contracts.FirstOrDefault(c => c.Contractnumber == payment.Contractnumber);
                var account = _context.Accounts.FirstOrDefault(c => c.Customerid == contract.Customerid);

                if (contract != null)
                {
                    contract.Constatus = 5; // Đã thanh toán
                    _context.Contracts.Update(contract);

                    var statusHistory = new ContractStatusHistory
                    {
                        Contractnumber = payment.Contractnumber,
                        OldStatus = 4,
                        NewStatus = 5,
                        ChangedAt = DateTime.Now,
                        ChangedBy = account.Rootaccount
                    };
                    _context.ContractStatusHistories.Add(statusHistory);

                    var oldContractFile = _context.ContractFiles.FirstOrDefault(c => c.Contractnumber == payment.Contractnumber && c.FileStatus == 3);
                    if (oldContractFile != null)
                    {
                        var newContractFile = new ContractFile
                        {
                            Contractnumber = payment.Contractnumber,
                            ConfileName = oldContractFile.ConfileName,
                            FilePath = oldContractFile.FilePath,
                            UploadedAt = DateTime.Now,
                            FileStatus = 5
                        };
                        _context.ContractFiles.Add(newContractFile);
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

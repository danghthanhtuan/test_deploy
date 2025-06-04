using Microsoft.EntityFrameworkCore;
using System;
using System.Transactions;
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

        public bool ThanhToan(string maHopDong, string maGiaoDich, string email, string phuongThuc)
        {
            var payment = _context.Payments.FirstOrDefault(p =>
                p.Contractnumber == maHopDong && p.Paymentstatus == false);

            if (payment == null) return false;

            //payment.PaymentDate = DateTime.Now;
            //payment.PaymentMethod = phuongThuc;
            //payment.Paymentstatus = true;
            //payment.TransactionCode = maGiaoDich;
            //_context.Payments.Update(payment);

            var newPayment = new PaymentTransaction
            {
                Id = payment.Id,
                TransactionCode = maGiaoDich,
                PaymentDate = DateTime.Now,
                PaymentMethod = phuongThuc,
                PaymentResult = true,
                Amount = payment.Amount,
            };
            _context.PaymentTransactions.Add(newPayment);


            var contract = _context.Contracts.FirstOrDefault(c => c.Contractnumber == maHopDong);
            if (contract != null)
            {
                contract.Constatus = 4;
            }
            _context.Contracts.Update(contract);

            var newContractStatusHistory = new ContractStatusHistory
            {
                Contractnumber = maHopDong,
                OldStatus = 3,
                NewStatus = 4,
                ChangedAt = DateTime.Now,
                ChangedBy = email
            };
            _context.ContractStatusHistories.Add(newContractStatusHistory);

            var contractfile = _context.ContractFiles.FirstOrDefault(c => c.Contractnumber == maHopDong);
            var contractfileS = new ContractFile
            {
                Contractnumber = maHopDong,
                ConfileName = contractfile.ConfileName, 
                FilePath = contractfile.FilePath,
                UploadedAt = DateTime.Now,
                FileStatus = 4
            };
            _context.ContractFiles.Add(contractfileS);
            _context.SaveChanges();
            return true;
        }
    }
}

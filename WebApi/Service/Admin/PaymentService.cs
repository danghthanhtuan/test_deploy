using Microsoft.EntityFrameworkCore;
using System;
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

            payment.PaymentDate = DateTime.Now;
            payment.PaymentMethod = phuongThuc;
            payment.Paymentstatus = true;
            payment.TransactionCode = maGiaoDich;
            _context.Payments.Update(payment);

            var contract = _context.Contracts.FirstOrDefault(c => c.Contractnumber == maHopDong);
            if (contract != null)
            {
                contract.Constatus = "Đã thanh toán";
            }
            _context.Contracts.Update(contract);

            var newContractStatusHistory = new ContractStatusHistory
            {
                Contractnumber = maHopDong,
                OldStatus = "Ký hoàn tất",
                NewStatus = "Đã thanh toán",
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
                FileStatus = "Đã thanh toán"
            };
            _context.ContractFiles.Add(contractfileS);
            _context.SaveChanges();
            return true;
        }
    }
}

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

        public bool ThanhToan(string ID, string maGiaoDich,  string phuongThuc, string tinhTrang)
        {
            if (!int.TryParse(ID, out int idInt))
            {
                return false;
            }

            var paymenttransaction = _context.PaymentTransactions
                .FirstOrDefault(p => p.Id == idInt);

            var payment = _context.Payments.FirstOrDefault(p=> p.Id == paymenttransaction.PaymentId);
            if (payment == null) return false;

            bool PaymentResultint;
            if (tinhTrang =="Thanh cong")
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
                    contract.Constatus = 4; // Đã thanh toán
                    _context.Contracts.Update(contract);    

                    var statusHistory = new ContractStatusHistory
                    {
                        Contractnumber = payment.Contractnumber,
                        OldStatus = 3,
                        NewStatus = 4,
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
                            FileStatus = 4
                        };
                        _context.ContractFiles.Add(newContractFile);
                    }
                }
            }

            _context.SaveChanges();
            return true;
        }
    }
}

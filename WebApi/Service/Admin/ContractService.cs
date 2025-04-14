using WebApi.DTO;
using WebApi.Models;
using WebApi.Service.Client;

namespace WebApi.Service.Admin
{
    public class ContractService
    {
        private readonly ManagementDbContext _context;
        
        public ContractService(ManagementDbContext context )
        {
            _context = context;
        }
        public string? Insert(ContractDTO contractDTO, string id)
        {
            if (contractDTO == null || contractDTO.chooseMonth <= 0)
            {
                return "Dữ liệu không hợp lệ.";
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var staff = _context.Staff.FirstOrDefault(s => s.Staffid == id);
                    if (staff == null)
                    {
                        return $"Nhân viên với mã nhân viên = {id} không tồn tại";
                    }
                    var oldContract = _context.Contracts
                        .FirstOrDefault(c => c.Contractnumber == contractDTO.ContractNumber);

                    if (oldContract != null && oldContract.Enddate >= DateTime.Today)
                    {
                        oldContract.Enddate = oldContract.Enddate
                            .AddMonths(contractDTO.chooseMonth)
                            .AddDays(-1);

                        _context.Contracts.Update(oldContract);

                        var newPayment = new Payment
                        {
                            Customerid = contractDTO.CustomerId,
                            Contractnumber = oldContract.Contractnumber,
                            Amount = contractDTO.Amount,
                            Paymentstatus = false,
                        };
                        _context.Payments.Add(newPayment);

                        _context.SaveChanges();
                        transaction.Commit();
                        return null;
                    }
                    else
                    {
                        DateTime startDate = DateTime.Today;
                        DateTime endDate = startDate.AddMonths(contractDTO.chooseMonth).AddDays(-1);
                        string? originalContractNumber = oldContract?.Contractnumber;

                        var lastContract = _context.Contracts
                            .OrderByDescending(c => c.Contractnumber)
                            .FirstOrDefault();

                        int nextContractNumber = lastContract != null
                            ? int.Parse(lastContract.Contractnumber.Substring(2)) + 1
                            : 1;

                        string newContractNumber = $"SV{nextContractNumber:D4}";

                        var newContract = new Contract
                        {
                            Contractnumber = newContractNumber,
                            Startdate = startDate,
                            Enddate = endDate,
                            ServiceTypename = contractDTO.ServiceType,
                            Customerid = contractDTO.CustomerId,
                            Original = originalContractNumber
                        };

                        var newPayment = new Payment
                        {
                            Customerid = contractDTO.CustomerId,
                            Contractnumber = newContractNumber,
                            Amount = contractDTO.Amount,
                            Paymentstatus = false,
                        };
                        _context.Contracts.Add(newContract);
                        _context.Payments.Add(newPayment);
                        _context.SaveChanges();

                        transaction.Commit();
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Lỗi hệ thống: {ex.Message}");
                    return "Lỗi hệ thống, vui lòng thử lại sau.";
                }
            }
        }


    }
}

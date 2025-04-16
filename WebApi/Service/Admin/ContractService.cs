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

                    if (oldContract == null)
                    {
                        return $"Hợp đồng với mã {contractDTO.ContractNumber} không tồn tại.";
                    }

                    // Tạo số hợp đồng mới
                    var lastContract = _context.Contracts
                        .OrderByDescending(c => c.Contractnumber)
                        .FirstOrDefault();

                    int nextContractNumber = lastContract != null
                        ? int.Parse(lastContract.Contractnumber.Substring(2)) + 1
                        : 1;

                    string newContractNumber = $"SV{nextContractNumber:D4}";

                    // Tìm loại dịch vụ
                    var serviceType = _context.ServiceTypes
                        .FirstOrDefault(st => st.ServiceTypename == contractDTO.ServiceType);

                    if (serviceType == null)
                    {
                        return $"Loại dịch vụ '{contractDTO.ServiceType}' không tồn tại.";
                    }

                    DateTime newStartDate;
                    DateTime newEndDate;
                    string? originalContract = null;

                    if (oldContract.Enddate < DateTime.Today)
                    {
                        // Hợp đồng đã hết hạn → tạo mới
                        newStartDate = DateTime.Today;
                        newEndDate = newStartDate.AddMonths(contractDTO.chooseMonth);
                        originalContract = null;
                    }
                    else
                    {
                        // Hợp đồng còn hạn → gia hạn từ ngày hết hạn cũ + 1 đến ngày người dùng chọn
                        newStartDate = oldContract.Enddate.AddDays(1);
                        newEndDate = contractDTO.Enddate;
                        originalContract = oldContract.Contractnumber;
                    }
                    var newContract = new Contract
                    {
                        Contractnumber = newContractNumber,
                        Startdate = newStartDate,
                        Enddate = newEndDate,
                        ServiceTypeid = serviceType.Id,
                        Customerid = contractDTO.CustomerId,
                        Original = originalContract
                    };

                    var newPayment = new Payment
                    {
                        Contractnumber = newContractNumber,
                        Amount = contractDTO.Amount,
                        Paymentstatus = false
                    };

                    _context.Contracts.Add(newContract);
                    _context.Payments.Add(newPayment);
                    _context.SaveChanges();
                    transaction.Commit();

                    return null;
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

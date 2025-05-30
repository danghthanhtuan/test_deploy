using iText.Kernel.Pdf;
using iText.Signatures;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Pkcs;
using WebApi.DTO;
using WebApi.Models;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace WebApi.Service.Admin
{
    public class SeeContract_SignService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ManagementDbContext _context;

        public SeeContract_SignService(IWebHostEnvironment env, ManagementDbContext context)
        {
            _env = env;
            _context = context;
        }

        //Cập nhật khi client ký xong
        public async Task<string> UploadSignedContract(IFormFile signedFile, string email, string originalFileName)
        {
            // Bắt lỗi toàn bộ quá trình
            try
            {
                // 1. Tạo thư mục lưu file nếu chưa có
                var folderPath = Path.Combine(
                    _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                    "signed-contracts"
                );
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // 2. Tạo tên file mới
                var fileName = $"{Path.GetFileNameWithoutExtension(signedFile.FileName)}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                var filePath = Path.Combine(folderPath, fileName);

                // 3. Lưu file tạm thời
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await signedFile.CopyToAsync(stream);
                }

                // 4. Kiểm tra email có tồn tại
                var account = await _context.Accounts.FirstOrDefaultAsync(s => s.Rootaccount == email);
                if (account == null)
                    return "Email khách hàng không tồn tại trong hệ thống.";

                // 5. Tìm hợp đồng theo CustomerId
                var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.Customerid == account.Customerid);
                if (contract == null)
                    return "Không tìm thấy hợp đồng tương ứng với khách hàng.";

                // 6. Bắt đầu transaction
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // 7. Cập nhật trạng thái hợp đồng
                    contract.Constatus = "Đã ký";
                    _context.Contracts.Update(contract);

                    // 8. Thêm thông tin file
                    var newContractfile = new ContractFile
                    {
                        Contractnumber = contract.Contractnumber,
                        ConfileName = fileName,
                        FilePath = filePath,
                        UploadedAt = DateTime.Now,
                        FileStatus = "Đã ký",
                    };
                    _context.ContractFiles.Add(newContractfile);

                    // 9. Thêm lịch sử trạng thái
                    var newContractStatusHistory = new ContractStatusHistory
                    {
                        Contractnumber = contract.Contractnumber,
                        OldStatus = "Chưa ký", // hoặc có thể truy vấn trạng thái cũ nếu muốn
                        NewStatus = "Đã ký",
                        ChangedAt = DateTime.Now,
                        ChangedBy = email,
                    };
                    _context.ContractStatusHistories.Add(newContractStatusHistory);

                    // 10. Lưu và commit
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // 11. Xoá file tạm ở temp-pdfs nếu có
                    var tempPdfPath = Path.Combine(
                        _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                        "temp-pdfs",
                        originalFileName
                    );
                    if (System.IO.File.Exists(tempPdfPath))
                        System.IO.File.Delete(tempPdfPath);
                    return fileName;
                }
                catch (Exception dbEx)
                {
                    await transaction.RollbackAsync();

                    // Xoá file đã lưu nếu có lỗi DB
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);

                    Console.WriteLine($"[DB ERROR] {dbEx.Message}");
                    return "Có lỗi xảy ra trong quá trình lưu dữ liệu. Đã rollback.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GENERAL ERROR] {ex.Message}");
                return "Lỗi khi xử lý file hoặc kết nối cơ sở dữ liệu.";
            }
        }

        public async Task<(bool Success, string Message)> SaveSignedPdfAsync(IFormFile signedFile, string fileName, string email)
        {
            try
            {
                // 1. Đường dẫn thư mục lưu file (cố định theo yêu cầu)
                var folderPath = @"D:\DATN\WebSystemManagement\WebApi\wwwroot\signed-contracts";

                // 2. Tạo thư mục nếu chưa có
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // 3. Tạo đường dẫn đầy đủ của file
                var filePath = Path.Combine(folderPath, fileName);

                // 4. Lưu file ghi đè
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await signedFile.CopyToAsync(stream);
                }

                // 5. Kiểm tra email có tồn tại
                var account = await _context.Accounts.FirstOrDefaultAsync(s => s.Rootaccount == email);
                if (account == null)
                    return (false, "Email khách hàng không tồn tại trong hệ thống.");

                // 6. Tìm hợp đồng theo CustomerId
                var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.Customerid == account.Customerid);
                if (contract == null)
                    return (false, "Không tìm thấy hợp đồng tương ứng với khách hàng.");

                // 7. Bắt đầu transaction
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // 8. Cập nhật trạng thái hợp đồng
                    contract.Constatus = "Ký hoàn tất";
                    _context.Contracts.Update(contract);

                    // 9. Thêm thông tin file
                    var newContractfile = new ContractFile
                    {
                        Contractnumber = contract.Contractnumber,
                        ConfileName = fileName,
                        FilePath = filePath,
                        UploadedAt = DateTime.Now,
                        FileStatus = "Ký hoàn tất",
                    };
                    _context.ContractFiles.Add(newContractfile);

                    // 10. Thêm lịch sử trạng thái
                    var newContractStatusHistory = new ContractStatusHistory
                    {
                        Contractnumber = contract.Contractnumber,
                        OldStatus = "Chờ client ký",
                        NewStatus = "Ký hoàn tất",
                        ChangedAt = DateTime.Now,
                        ChangedBy = email,
                    };
                    _context.ContractStatusHistories.Add(newContractStatusHistory);

                    // 11. Lưu và commit
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "Lưu file và cập nhật DB thành công.");
                }
                catch (Exception dbEx)
                {
                    await transaction.RollbackAsync();

                    // Xoá file đã lưu nếu có lỗi DB
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);

                    Console.WriteLine($"[DB ERROR] {dbEx.Message}");
                    return (false, "Có lỗi xảy ra trong quá trình lưu dữ liệu. Đã rollback.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi lưu file hoặc cập nhật DB: {ex.Message}");
            }
        }

    }
}

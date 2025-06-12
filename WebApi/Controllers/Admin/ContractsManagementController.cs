using AutoMapper;
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
    public class ContractsManagementController : ControllerBase
    {
        private readonly ManagementDbContext _context;
        private readonly IMapper _mapper;
        private readonly ContractsManagementService _contractService;
        private readonly AccountService _accountService;
        private readonly PdfService _pdfService;

        public ContractsManagementController(IMapper mapper, ManagementDbContext context, ContractsManagementService contractService, AccountService accountService, PdfService pdfService)
        {
            _mapper = mapper;
            _contractService = contractService;
            _context = context;
            _accountService = accountService;
            _pdfService = pdfService;
        }

        //Lấy thông tin công ty chính thức
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public async Task<ActionResult<CompanyAccountDTO>> GetAllCompany([FromBody] GetListCompanyPaging req)
        {
            var company = await _contractService.GetAllCompany(req);
            return Ok(company);
        }
       
        //Gia hạn hợp đồng
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public IActionResult InsertExtend([FromBody] ContractDTO contractDTO, [FromQuery] string id)
        {
            if (contractDTO == null || string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var result = _contractService.InsertExtend(contractDTO, id);

            if (result == null)
            {
                return Ok(new
                {
                    success = true,
                    message = "Gia hạn tài khoản thành công",
                    companyID = result
                });
            }
            else
            {
                return BadRequest(new { success = false, message = result });
            }

        }

        //Tạo file hợp đồng mới
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public async Task<IActionResult> GenerateContract([FromBody] CompanyAccountDTO dto, [FromQuery] string id)
        {
            string contractId = Guid.NewGuid().ToString();

            // 1. Tạo file PDF gốc chưa ký
            byte[] pdfBytes = _pdfService.GenerateContractPdf(dto);

            // 2. Lưu file PDF chờ boss ký
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp-pdfs");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = $"{contractId}.pdf";
            string fullPath = Path.Combine(folderPath, fileName);
            //await System.IO.File.WriteAllBytesAsync(fullPath, signedPdfBytes);
            await System.IO.File.WriteAllBytesAsync(fullPath, pdfBytes);

            string relativePath = Path.Combine("/temp-pdfs", fileName).Replace("\\", "/");

            // 5. Lưu thông tin hợp đồng, trạng thái file đã ký vào DB
            var result = await _contractService.SaveContractStatusAsync(new CompanyContractDTOs
            {
                CustomerId = dto.CustomerId,
                CompanyName = dto.CompanyName,
                TaxCode = dto.TaxCode,
                CompanyAccount = dto.CompanyAccount,
                AccountIssuedDate = dto.AccountIssuedDate,
                CPhoneNumber = dto.CPhoneNumber,
                CAddress = dto.CAddress,
                RootAccount = dto.RootAccount,
                RootName = dto.RootName,
                RPhoneNumber = dto.RPhoneNumber,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                ContractNumber = dto.ContractNumber,
                Startdate = dto.Startdate,
                Enddate = dto.Enddate,
                CustomerType = dto.CustomerType,
                ServiceType = dto.ServiceType,
                ConfileName = fileName,
                FilePath = relativePath,
                ChangedBy = id,
                Amount = dto.Amount,
                Original = dto.Original,
            });

            if (result == null || result.StartsWith("Lỗi") || result.Contains("không tồn tại") || result.Contains("đã tồn tại"))
            {
                return BadRequest(new { success = false, message = result });
            }
            return Ok(new { success = true, fullPath });

        }

        //nâng cấp hợp đồng 
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public IActionResult InsertUpgrade([FromBody] ContractDTO contractDTO, [FromQuery] string id)
        {
            if (contractDTO == null || string.IsNullOrEmpty(id))
            {
                Console.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var result = _contractService.InsertUpgrade(contractDTO, id);

            if (result == null)
            {
                return Ok(new
                {
                    success = true,
                    message = "Nâng cấp tài khoản thành công",
                    companyID = result
                });
            }
            else
            {
                return BadRequest(new { success = false, message = result });
            }

        }

        //Lấy ưu đãi đang có hiệu lực theo nhóm dịch vụ của hợp đồng
        [Authorize(Policy = "AdminPolicy")]
        [HttpGet]
        public async Task<ActionResult<Endow>> GetListEndow([FromQuery] string id)
        {
            var endow = await _contractService.GetListEndow(id);
            return Ok(endow);
        }

        [HttpGet]
        public async Task<ActionResult<CompanyAccountDTO>> GetAllInfor([FromQuery] string req)
        {
            var company = await _contractService.GetAllInfor(req);
            return Ok(company);
        }

        //Tạo file hợp đồng gia hạn
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public async Task<IActionResult> GenerateContractExtend([FromBody] ContractDTO contractDTO, [FromQuery] string id)
        {
            string contractId = Guid.NewGuid().ToString();

            var dto = await _contractService.GetByContractNumberAsync(contractDTO.ContractNumber);

            if (dto == null)
                return NotFound("Không tìm thấy thông tin hợp đồng gốc.");
            var (startDate, endDate, original) = _contractService.CalculateNewContractPeriod(contractDTO);

            dto.Startdate = startDate;
            dto.Enddate = endDate;
            dto.Original = original;

            // Kiểm tra có hợp đồng gia hạn chưa hoàn tất
            var exist = await _contractService.CheckExistingIncompleteExtension(contractDTO.ContractNumber);
            if (exist != null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Hợp đồng {contractDTO.ContractNumber} đã có bản hợp đồng cập nhật ({exist}) chưa hoàn tất, không thể tiếp tục gia hạn."
                });
            }

            // 1. Tạo file PDF gốc chưa ký
            byte[] pdfBytes = _pdfService.GenerateContractPdf(dto);

            // 2. Lưu file PDF chờ boss ký
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp-pdfs");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = $"{contractId}.pdf";
            string fullPath = Path.Combine(folderPath, fileName);
            //await System.IO.File.WriteAllBytesAsync(fullPath, signedPdfBytes);
            await System.IO.File.WriteAllBytesAsync(fullPath, pdfBytes);

            string relativePath = Path.Combine("/temp-pdfs", fileName).Replace("\\", "/");

            var value = new CompanyContractDTOs
            {
                CustomerId = dto.CustomerId,
                CompanyName = dto.CompanyName,
                TaxCode = dto.TaxCode,
                CompanyAccount = dto.CompanyAccount,
                //AccountIssuedDate = dto.AccountIssuedDate,
                CPhoneNumber = dto.CPhoneNumber,
                CAddress = dto.CAddress,
                RootAccount = dto.RootAccount,
                RootName = dto.RootName,
                RPhoneNumber = dto.RPhoneNumber,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                ContractNumber = contractDTO.ContractNumber,
                Startdate = dto.Startdate,
                Enddate = dto.Enddate,
                CustomerType = dto.CustomerType,
                ServiceType = dto.ServiceType,
                ConfileName = fileName,
                FilePath = relativePath,
                ChangedBy = id,
                Amount = dto.Amount,
                Original = dto.Original,
            };
            // 5. Lưu thông tin hợp đồng, trạng thái file đã ký vào DB
            var result = await _contractService.SaveContractExtend(value, contractDTO, id);

            if (result == null || result.StartsWith("Lỗi") || result.Contains("không tồn tại") || result.Contains("đã tồn tại"))
            {
                return BadRequest(new { success = false, message = result });
            }
            return Ok(new { success = true, fullPath });

        }

        //Tạo file hợp đồng nâng cấp
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        public async Task<IActionResult> GenerateContractUpgrade([FromBody] ContractDTO contractDTO, [FromQuery] string id)
        {
            string contractId = Guid.NewGuid().ToString();

            var dto = await _contractService.GetByContractNumberAsync(contractDTO.ContractNumber);

            if (dto == null)
                return NotFound("Không tìm thấy thông tin hợp đồng gốc.");

            // Kiểm tra có hợp đồng gia hạn chưa hoàn tất
            var exist = await _contractService.CheckExistingIncompleteExtension(contractDTO.ContractNumber);
            if (exist != null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"Hợp đồng {contractDTO.ContractNumber} đã có bản hợp đồng cập nhật ({exist}) chưa hoàn tất, không thể tiếp tục nâng cấp."
                });
            }

            // 1. Tạo file PDF gốc chưa ký
            byte[] pdfBytes = _pdfService.GenerateContractPdf(dto);

            // 2. Lưu file PDF chờ boss ký
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp-pdfs");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = $"{contractId}.pdf";
            string fullPath = Path.Combine(folderPath, fileName);
            //await System.IO.File.WriteAllBytesAsync(fullPath, signedPdfBytes);
            await System.IO.File.WriteAllBytesAsync(fullPath, pdfBytes);

            string relativePath = Path.Combine("/temp-pdfs", fileName).Replace("\\", "/");

            var value = new CompanyContractDTOs
            {
                CustomerId = dto.CustomerId,
                CompanyName = dto.CompanyName,
                TaxCode = dto.TaxCode,
                CompanyAccount = dto.CompanyAccount,
                CPhoneNumber = dto.CPhoneNumber,
                CAddress = dto.CAddress,
                RootAccount = dto.RootAccount,
                RootName = dto.RootName,
                RPhoneNumber = dto.RPhoneNumber,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                ContractNumber = contractDTO.ContractNumber,
                Startdate = dto.Startdate,
                Enddate = dto.Enddate,
                CustomerType = contractDTO.Customertype,
                ServiceType = dto.ServiceType,
                ConfileName = fileName,
                FilePath = relativePath,
                ChangedBy = id,
                Amount = contractDTO.Amount,
                Original = contractDTO.ContractNumber,
            };
            // 5. Lưu thông tin hợp đồng, trạng thái file đã ký vào DB
            var result = await _contractService.SaveContractUpgrade(value, id);

            if (result == null || result.StartsWith("Lỗi") || result.Contains("không tồn tại") || result.Contains("đã tồn tại"))
            {
                return BadRequest(new { success = false, message = result });
            }
            return Ok(new { success = true, fullPath });

        }

    }
}

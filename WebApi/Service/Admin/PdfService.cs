using WebApi.DTO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using QuestPDF.Elements;

namespace WebApi.Service.Admin
{
    public class PdfService
    {
        public byte[] GenerateContractPdf(CompanyAccountDTO dto)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                    page.Header().Element(header =>
                    {
                        header.Text("HỢP ĐỒNG CUNG CẤP DỊCH VỤ")
                              .FontSize(16)
                              .Bold()
                              .AlignCenter();

                        header.PaddingBottom(10); // hoặc dùng MarginBottom nếu phù hợp
                    });


                    page.Content().Column(column =>
                    {
                        column.Item().Text($"Mã công ty: {dto.CompanyAccount}");
                        column.Item().Text($"Tên công ty: {dto.CompanyName}");
                        column.Item().Text($"Địa chỉ: {dto.CAddress}");
                        column.Item().Text($"Người đại diện: {dto.RootName}");
                        column.Item().Text($"Email: {dto.RootAccount}");
                        column.Item().Text($"Số điện thoại: {dto.RPhoneNumber}");

                        column.Item().PaddingVertical(15).Text("Điều khoản hợp đồng:")
                            .FontSize(14).Bold();

                        column.Item().Text("1. Hai bên đồng ý cung cấp và sử dụng dịch vụ theo điều kiện được nêu trong hợp đồng.");
                        column.Item().Text("2. Thời hạn hợp đồng: 12 tháng kể từ ngày ký.");
                        column.Item().Text("3. Các điều khoản khác theo quy định của pháp luật Việt Nam.");

                        column.Item().PaddingVertical(20).Text("Ngày lập hợp đồng: " + DateTime.Now.ToString("dd/MM/yyyy"));

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Đại diện Bên A").AlignCenter();
                            row.RelativeItem().Text("Đại diện Bên B").AlignCenter();
                        });

                        column.Item().Height(100); // chừa khoảng trống để ký

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("(Ký, ghi rõ họ tên)").AlignCenter();
                            row.RelativeItem().Text("(Ký, ghi rõ họ tên)").AlignCenter();
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Trang được tạo bởi hệ thống").FontSize(10);
                    });
                });
            });

            // Trả về PDF dưới dạng mảng byte
            return document.GeneratePdf();
        }
    }
}

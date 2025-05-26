using WebApi.DTO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using iText.Kernel.Pdf;
using iText.Signatures;
using iText.Kernel.Geom;
using Org.BouncyCastle.Pkcs;
using iText.Bouncycastle.X509;
using iText.Bouncycastle.Crypto;
using iText.Commons.Bouncycastle.Cert;
using Org.BouncyCastle.Crypto;
using iText.Forms;
using iText.Forms.Fields;
using iText.Forms;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.IO.Font.Constants;
using iText.Kernel.Font;

namespace WebApi.Service.Admin
{
    public class PdfService
    {
        public byte[] GenerateContractPdf(CompanyAccountDTO dto)
        {
            QuestPDF.Settings.License = LicenseType.Community;

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
                        header
                            .PaddingBottom(10)
                            .Text("HỢP ĐỒNG CUNG CẤP DỊCH VỤ")
                            .FontSize(16)
                            .Bold()
                            .AlignCenter();
                    });

                    page.Content().Column(column =>
                    {
                        column.Item().PaddingBottom(10).Column(col =>
                        {
                            col.Item().Text("🔹 Thông tin công ty").Bold().FontSize(14).Underline();
                            col.Item().Text($"Tên công ty: {dto.CompanyName}");
                            col.Item().Text($"Mã số thuế: {dto.TaxCode}");
                            col.Item().Text($"Email công ty: {dto.CompanyAccount}");
                            col.Item().Text($"Số điện thoại: {dto.CPhoneNumber}");
                            col.Item().Text($"Địa chỉ: {dto.CAddress}");

                            col.Item().PaddingTop(10).Text("🔹 Người đại diện").Bold().FontSize(14).Underline();
                            col.Item().Text($"Họ tên: {dto.RootName}");
                            col.Item().Text($"Email: {dto.RootAccount}");
                            col.Item().Text($"Số điện thoại: {dto.RPhoneNumber}");

                            col.Item().PaddingTop(10).Text("🔹 Dịch vụ").Bold().FontSize(14).Underline();
                            col.Item().Text($"Phân loại: {dto.CustomerType}");
                            col.Item().Text($"Loại dịch vụ: {dto.ServiceType}");
                            col.Item().Text($"Ngày bắt đầu: {dto.Startdate:dd/MM/yyyy}");
                            col.Item().Text($"Ngày kết thúc: {dto.Enddate:dd/MM/yyyy}");
                            col.Item().Text($"Giá: {dto.Amount:N0} VND");
                        });


                        column.Item().PaddingVertical(15).Text("Điều khoản hợp đồng:").FontSize(14).Bold();

                        column.Item().Text("1. Hai bên thống nhất rằng Bên B sẽ cung cấp dịch vụ \"" + dto.ServiceType + "\" cho Bên A theo các điều kiện sau:");

                        column.Item().Text("2. Phạm vi cung cấp dịch vụ:");
                        column.Item().Text("   - Bên B cung cấp đầy đủ dịch vụ với nội dung, phạm vi và thông số kỹ thuật theo mô tả hoặc yêu cầu từ Bên A.");
                        column.Item().Text("   - Các điều chỉnh, mở rộng hoặc thay đổi phạm vi dịch vụ sẽ được hai bên thống nhất bằng văn bản hoặc phụ lục hợp đồng.");

                        column.Item().Text("3. Thời hạn và hiệu lực:");
                        column.Item().Text($"   - Hợp đồng có hiệu lực từ ngày {dto.Startdate:dd/MM/yyyy} đến ngày {dto.Enddate:dd/MM/yyyy}");

                        column.Item().Text("   - Hai bên có thể gia hạn hợp đồng trước khi hết hạn tối thiểu 15 ngày bằng văn bản.");

                        column.Item().Text("4. Giá trị và thanh toán:");
                        column.Item().Text("   - Tổng giá trị hợp đồng: " + dto.Amount + ".");
                        column.Item().Text("   - Bên A thanh toán cho Bên B theo phương thức và thời hạn được quy định trong phụ lục hợp đồng hoặc hóa đơn.");

                        column.Item().Text("5. Hỗ trợ và bảo trì:");
                        column.Item().Text("   - Bên B cung cấp các hình thức hỗ trợ sau:");
                        column.Item().Text("     + Hỗ trợ kỹ thuật: xử lý sự cố, hướng dẫn vận hành, khôi phục dịch vụ.");
                        column.Item().Text("     + Hỗ trợ cước phí: đối soát, giải trình hóa đơn, điều chỉnh phí.");
                        column.Item().Text("     + Bảo hành thiết bị (nếu có): theo chính sách nhà sản xuất hoặc thỏa thuận.");
                        column.Item().Text("     + Cập nhật dịch vụ: thay đổi cấu hình, nâng cấp hoặc bổ sung tính năng.");
                        column.Item().Text("   - Kênh hỗ trợ: Email, điện thoại, cổng hỗ trợ trực tuyến.");

                        column.Item().Text("6. Trách nhiệm các bên:");
                        column.Item().Text("   - Bên B đảm bảo chất lượng dịch vụ theo cam kết và chịu trách nhiệm khắc phục sự cố phát sinh.");
                        column.Item().Text("   - Bên A phối hợp cung cấp thông tin, dữ liệu và điều kiện kỹ thuật cần thiết để triển khai dịch vụ.");
                        column.Item().Text("   - Hai bên cam kết bảo mật thông tin, không tiết lộ nội dung hợp đồng cho bên thứ ba nếu không có sự đồng ý.");

                        column.Item().Text("7. Điều khoản khác:");
                        column.Item().Text("   - Mọi tranh chấp phát sinh trong quá trình thực hiện hợp đồng sẽ được giải quyết trên tinh thần hợp tác. Trường hợp không thỏa thuận được sẽ đưa ra Tòa án có thẩm quyền.");
                        column.Item().Text("   - Hợp đồng này được lập thành 02 bản có giá trị pháp lý như nhau, mỗi bên giữ 01 bản.");

                        column.Item().PaddingVertical(20).Text("Ngày lập hợp đồng: " + DateTime.Now.ToString("dd/MM/yyyy"));

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Đại diện Bên A").AlignCenter();
                            row.RelativeItem().Text("Đại diện Bên B").AlignCenter();
                        });

                        // Tạo ô ký vẽ bằng khung viền
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(c => c.Border(1).Height(100).AlignCenter().Padding(5).AlignCenter());
                            row.RelativeItem().Element(c => c.Border(1).Height(100).AlignCenter().Padding(5).AlignCenter());
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Trang được tạo bởi hệ thống ").FontSize(10);
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] SignPdfWithAdminCertificate(byte[] originalPdfBytes, string staffId)
        {
            string pfxPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "certificates", "demo_test.pfx");
            string pfxPassword = "123456";

            // Load certificate
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();
            using (FileStream fs = new FileStream(pfxPath, FileMode.Open, FileAccess.Read))
            {
                store.Load(fs, pfxPassword.ToCharArray());
            }

            string alias = store.Aliases.Cast<string>().FirstOrDefault(store.IsKeyEntry);
            AsymmetricKeyParameter privateKey = store.GetKey(alias).Key;

            var chain = store.GetCertificateChain(alias)
                .Select(c => new X509CertificateBC(c.Certificate))
                .Cast<IX509Certificate>()
                .ToList();

            var iPrivateKey = new PrivateKeyBC(privateKey);

            using var signedPdfStream = new MemoryStream();
            using var originalPdfStream = new MemoryStream(originalPdfBytes);
            var reader = new PdfReader(originalPdfStream);
            var signer = new PdfSigner(reader, signedPdfStream, new StampingProperties());

            // Vị trí chữ ký (tọa độ tính từ bottom-left)
            //Rectangle rect = new Rectangle(100, 100, 200, 100);
            //(string keyword, float offsetY) = ("Đại diện Bên B", -50f);
            //var (textRect, page) = FindTextPosition(originalPdfBytes, keyword);
            //float offsetX = -10f; // Dịch trái một chút
            (string keyword, float offsetY) = ("Đại diện Bên B", 90f);
            var (textRect, page) = FindTextPosition(originalPdfBytes, keyword);

            // Dịch trái nhiều hơn → từ -10f → -20f (hoặc -30f nếu cần)
            float offsetX = -20f;

            // Giảm chiều rộng và chiều cao khung chữ ký
            float signatureWidth = 200f;  // từ 240 → 200 (hoặc 180 nếu cần)
            float signatureHeight = 50f;  // từ 60 → 50

            Rectangle rect = new Rectangle(
                textRect.GetX() + offsetX,
                textRect.GetY() - offsetY,
                signatureWidth,
                signatureHeight
            );

            // Font và Appearance
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            var appearance = signer.GetSignatureAppearance();
            appearance
                .SetPageRect(rect)
                .SetPageNumber(page)
                .SetLocation("Hệ thống")
                .SetLayer2Font(font)
                .SetLayer2FontSize(9)
                .SetReason("Ký bởi Admin")
                .SetLayer2Text($"Ký bởi {staffId}\nNgày: {DateTime.Now:dd/MM/yyyy}")
                .SetRenderingMode(PdfSignatureAppearance.RenderingMode.DESCRIPTION);

            signer.SetFieldName("Signature2");

            IExternalSignature externalSignature = new PrivateKeySignature(iPrivateKey, DigestAlgorithms.SHA256);
            IExternalDigest digest = new BouncyCastleDigest();

            signer.SignDetached(
                digest, externalSignature,
                chain.ToArray(), null, null, null,
                0, PdfSigner.CryptoStandard.CADES
            );

            return signedPdfStream.ToArray();

        }

        private (Rectangle rect, int page) FindTextPosition(byte[] pdfBytes, string keyword)
        {
            using var pdfReader = new PdfReader(new MemoryStream(pdfBytes));
            using var pdfDoc = new PdfDocument(pdfReader);

            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                var strategy = new TextLocationStrategy(keyword);
                var processor = new PdfCanvasProcessor(strategy);
                processor.ProcessPageContent(pdfDoc.GetPage(i));

                if (strategy.Locations.Any())
                {
                    return (strategy.Locations.First(), i);
                }
            }

            throw new Exception($"Không tìm thấy từ khóa '{keyword}' trong PDF.");
        }

    }
}

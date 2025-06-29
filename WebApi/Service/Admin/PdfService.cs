﻿using WebApi.DTO;
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
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Globalization;
using System.Text;
using iText.IO.Image;
using iText.Kernel.Pdf.Canvas;
using ImgSharp = SixLabors.ImageSharp;
using ImgProc = SixLabors.ImageSharp.Processing;
using ImgPixel = SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using System.Text.RegularExpressions;
using iText.IO.Font;


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
                            if(dto.CustomerType == true)
                                col.Item().Text($"Phân loại: VIP");
                            else
                                col.Item().Text($"Phân loại: Bình thường");
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

        //public byte[] SignPdfWithAdminCertificate(byte[] originalPdfBytes, string staffId)
        //{
        //    string pfxPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "certificates", "demo_test.pfx");
        //    string pfxPassword = "123456";

        //    // Load certificate
        //    Pkcs12Store store = new Pkcs12StoreBuilder().Build();
        //    using (FileStream fs = new FileStream(pfxPath, FileMode.Open, FileAccess.Read))
        //    {
        //        store.Load(fs, pfxPassword.ToCharArray());
        //    }

        //    string alias = store.Aliases.Cast<string>().FirstOrDefault(store.IsKeyEntry);
        //    AsymmetricKeyParameter privateKey = store.GetKey(alias).Key;

        //    var chain = store.GetCertificateChain(alias)
        //        .Select(c => new X509CertificateBC(c.Certificate))
        //        .Cast<IX509Certificate>()
        //        .ToList();

        //    var iPrivateKey = new PrivateKeyBC(privateKey);

        //    using var signedPdfStream = new MemoryStream();
        //    using var originalPdfStream = new MemoryStream(originalPdfBytes);
        //    var reader = new PdfReader(originalPdfStream);
        //    var signer = new PdfSigner(reader, signedPdfStream, new StampingProperties());

        //    (string keyword, float offsetY) = ("Đại diện Bên B", 80f);
        //    var (textRect, page) = FindTextPosition(originalPdfBytes, keyword);

        //    float offsetX = -40f;

        //    // Giảm chiều rộng và chiều cao khung chữ ký
        //    float signatureWidth = 200f;  // từ 240 → 200 (hoặc 180 nếu cần)
        //    float signatureHeight = 50f;  // từ 60 → 50

        //    Rectangle rect = new Rectangle(
        //        textRect.GetX() + offsetX,
        //        textRect.GetY() - offsetY,
        //        signatureWidth,
        //        signatureHeight
        //    );

        //    // Font và Appearance
        //    PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        //    var appearance = signer.GetSignatureAppearance();
        //    appearance
        //        .SetPageRect(rect)
        //        .SetPageNumber(page)
        //        .SetLocation("Hệ thống")
        //        .SetLayer2Font(font)
        //        .SetLayer2FontSize(9)
        //        .SetReason("Ký bởi Admin")
        //        .SetLayer2Text($"Ký bởi {staffId}\nNgày: {DateTime.Now:dd/MM/yyyy}")
        //        .SetRenderingMode(PdfSignatureAppearance.RenderingMode.DESCRIPTION);

        //    signer.SetFieldName("Signature2");

        //    IExternalSignature externalSignature = new PrivateKeySignature(iPrivateKey, DigestAlgorithms.SHA256);
        //    IExternalDigest digest = new BouncyCastleDigest();

        //    signer.SignDetached(
        //        digest, externalSignature,
        //        chain.ToArray(), null, null, null,
        //        0, PdfSigner.CryptoStandard.CADES
        //    );

        //    return signedPdfStream.ToArray();

        //}

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

        public byte[] SignPdfWithClientCertificate(byte[] originalPdfBytes, Stream clientPfxStream, string pfxPassword, string clientName)
        {
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();

            try
            {
                store.Load(clientPfxStream, pfxPassword.ToCharArray());
            }
            catch (Exception ex)
            {
                throw new Exception("Mật khẩu chứng thư số không chính xác hoặc file không hợp lệ.");
            }

            string alias = store.Aliases.Cast<string>().FirstOrDefault(store.IsKeyEntry);
            if (alias == null)
                throw new Exception("Không tìm thấy khóa bí mật trong file chứng thư số.");

            // Lấy khóa và chuỗi chứng chỉ
            AsymmetricKeyParameter privateKey = store.GetKey(alias).Key;

            var certChain = store.GetCertificateChain(alias);
            var chain = certChain
                .Select(c => new X509CertificateBC(c.Certificate))
                .Cast<IX509Certificate>()
                .ToList();

            var iPrivateKey = new PrivateKeyBC(privateKey);

            var cert = certChain[0].Certificate;
            DateTime now = DateTime.Now;

            if (now < cert.NotBefore.ToLocalTime() || now > cert.NotAfter.ToLocalTime())
            {
                throw new Exception("Chứng thư số đã hết hạn hoặc chưa có hiệu lực.");
            }

            string signerName = cert.SubjectDN.ToString();
            DateTime notBefore = cert.NotBefore.ToLocalTime();
            DateTime notAfter = cert.NotAfter.ToLocalTime();

            // Ký PDF
            using var signedPdfStream = new MemoryStream();
            using var originalPdfStream = new MemoryStream(originalPdfBytes);
            var reader = new PdfReader(originalPdfStream);
            var signer = new PdfSigner(reader, signedPdfStream, new StampingProperties());


            // Tìm vị trí chữ ký theo từ khóa
            (string keyword, float offsetY) = ("Đại diện Bên A", 113f);
            var (textRect, page) = FindTextPosition2(originalPdfBytes, keyword);

            float offsetX = 30f;
            float signatureWidth = 200f;
            float signatureHeight = 70f;

            Rectangle rect = new Rectangle(
                textRect.GetX() + offsetX,
                textRect.GetY() - offsetY,
                signatureWidth,
                signatureHeight
            );
            // Font chữ Unicode
            var fontPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "tahoma.ttf");
            PdfFont font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);

            string layerText = $"Ký bởi: {clientName}\nNgười dùng: {signerName}\nHiệu lực: {notBefore:dd/MM/yyyy} - {notAfter:dd/MM/yyyy}\nNgày ký: {DateTime.Now:dd/MM/yyyy}";

            var appearance = signer.GetSignatureAppearance();
            appearance
                .SetPageRect(rect)
                .SetPageNumber(page)
                .SetLocation("Client Upload")
                .SetLayer2Font(font)
                .SetLayer2FontSize(9f)
                .SetReason("Ký bởi Khách hàng")
                .SetLayer2Text(layerText)
                .SetRenderingMode(PdfSignatureAppearance.RenderingMode.DESCRIPTION);

            signer.SetFieldName("SignatureClient");

            IExternalSignature externalSignature = new PrivateKeySignature(iPrivateKey, DigestAlgorithms.SHA256);
            IExternalDigest digest = new BouncyCastleDigest();

            signer.SignDetached(
                digest, externalSignature,
                chain.ToArray(), null, null, null,
                0, PdfSigner.CryptoStandard.CADES
            );

            return signedPdfStream.ToArray();
        }
        private (Rectangle rect, int page) FindTextPosition2(byte[] pdfBytes, string keywordPart)
        {
            using var pdfReader = new PdfReader(new MemoryStream(pdfBytes));
            using var pdfDoc = new PdfDocument(pdfReader);

            string normalizedKeyword = NormalizeText(keywordPart);

            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                Console.WriteLine($"📄 Đang xử lý trang {i}");

                var page = pdfDoc.GetPage(i);
                var strategy = new GeneralTextLocationStrategy();

                var processor = new PdfCanvasProcessor(strategy);
                processor.ProcessPageContent(page);

                var locations = strategy.Locations;
                if (locations == null || locations.Count == 0)
                {
                    Console.WriteLine($"Không tìm thấy đoạn text nào ở trang {i}");
                    continue;
                }

                for (int j = 0; j < locations.Count; j++)
                {
                    string combinedText = "";
                    Rectangle? firstRect = null;

                    // Ghép tối đa 3 đoạn liên tiếp lại để tìm
                    for (int k = j; k < Math.Min(j + 3, locations.Count); k++)
                    {
                        if (string.IsNullOrWhiteSpace(locations[k].Text)) continue;

                        combinedText += locations[k].Text;
                        if (firstRect == null)
                            firstRect = locations[k].Rect;

                        string normalizedCombined = NormalizeText(combinedText);
                        Console.WriteLine($"Ghép đoạn: '{combinedText}' Normalized: '{normalizedCombined}'");

                        if (normalizedCombined.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"Tìm thấy từ khóa '{keywordPart}' tại trang {i}, vị trí x={firstRect.GetX()}, y={firstRect.GetY()}");
                            return (firstRect, i);
                        }
                    }
                }
            }

            throw new Exception($"❌ Không tìm thấy từ khóa gần đúng '{keywordPart}' trong PDF.");
        }


        // Helper để normalize text
        private string NormalizeText(string text)
        {
            return string.Concat(text.Where(c => !char.IsWhiteSpace(c)))
                 .ToLowerInvariant();
        }

        
        public byte[] InsertSignatureImageToPdf(byte[] originalPdfBytes, string signatureBase64, string keyword, float offsetX = 30f, float offsetY = 113f, float width = 200f, float height = 70f)
        {
            using var pdfStream = new MemoryStream(originalPdfBytes);
            using var outputPdfStream = new MemoryStream();

            // Decode base64 image
            var base64Data = Regex.Replace(signatureBase64, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);
            byte[] imageBytes = Convert.FromBase64String(base64Data);

            PdfReader reader = new PdfReader(pdfStream);
            PdfWriter writer = new PdfWriter(outputPdfStream);
            PdfDocument pdfDoc = new PdfDocument(reader, writer);
            iText.Layout.Document doc = new iText.Layout.Document(pdfDoc);

            // Tìm vị trí từ khóa để chèn ảnh
            (Rectangle textRect, int pageNum) = FindTextPosition2(originalPdfBytes, keyword); 
            float x = textRect.GetX() + offsetX;
            float y = textRect.GetY() - offsetY;

            // Tạo hình ảnh iText7
            iText.Layout.Element.Image signImg = new iText.Layout.Element.Image(ImageDataFactory.Create(imageBytes));
            signImg.SetFixedPosition(pageNum, x, y);
            signImg.SetWidth(width);
            signImg.SetHeight(height);

            // Chèn ảnh lên PDF
            doc.Add(signImg);

            doc.Close();
            return outputPdfStream.ToArray();
        }

        public byte[] SignPdfWithAdminCertificate(byte[] originalPdfBytes, Stream clientPfxStream, string pfxPassword, string staffId)
        {
            // Load PFX từ client
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();

            try
            {
                store.Load(clientPfxStream, pfxPassword.ToCharArray());
            }
            catch (Exception ex)
            {
                throw new Exception("Mật khẩu chứng thư số không chính xác hoặc file không hợp lệ.");
            }

            string alias = store.Aliases.Cast<string>().FirstOrDefault(store.IsKeyEntry);
            if (alias == null)
                throw new Exception("Không tìm thấy khóa bí mật trong file chứng thư số.");

            // Lấy khóa và chuỗi chứng chỉ
            AsymmetricKeyParameter privateKey = store.GetKey(alias).Key;

            var certChain = store.GetCertificateChain(alias);
            var chain = certChain
                .Select(c => new X509CertificateBC(c.Certificate))
                .Cast<IX509Certificate>()
                .ToList();

            var iPrivateKey = new PrivateKeyBC(privateKey);

            var cert = certChain[0].Certificate;
            DateTime now = DateTime.Now;

            if (now < cert.NotBefore.ToLocalTime() || now > cert.NotAfter.ToLocalTime())
            {
                throw new Exception("Chứng thư số đã hết hạn hoặc chưa có hiệu lực.");
            }

            string signerName = cert.SubjectDN.ToString();
            DateTime notBefore = cert.NotBefore.ToLocalTime();
            DateTime notAfter = cert.NotAfter.ToLocalTime();

            // Ký PDF
            using var signedPdfStream = new MemoryStream();
            using var originalPdfStream = new MemoryStream(originalPdfBytes);
            var reader = new PdfReader(originalPdfStream);
            var signer = new PdfSigner(reader, signedPdfStream, new StampingProperties());

            // Tìm vị trí ký
            (string keyword, float offsetY) = ("Đại diện Bên B", 80f);
            var (textRect, page) = FindTextPosition(originalPdfBytes, keyword);

            float offsetX = -40f;
            float signatureWidth = 200f;
            float signatureHeight = 70f;

            Rectangle rect = new Rectangle(
                textRect.GetX() + offsetX,
                textRect.GetY() - offsetY,
                signatureWidth,
                signatureHeight
            );

            // Font chữ Unicode
            var fontPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "tahoma.ttf");
            PdfFont font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);

            string layerText = $"Ký bởi: {staffId}\nNgười dùng: {signerName}\nHiệu lực: {notBefore:dd/MM/yyyy} - {notAfter:dd/MM/yyyy}\nNgày ký: {DateTime.Now:dd/MM/yyyy}";

            var appearance = signer.GetSignatureAppearance();
            appearance
                .SetPageRect(rect)
                .SetPageNumber(page)
                .SetLocation("Hệ thống")
                .SetLayer2Font(font)
                .SetLayer2FontSize(8)
                .SetReason("Ký bởi Admin")
                .SetLayer2Text(layerText)
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

    }
}

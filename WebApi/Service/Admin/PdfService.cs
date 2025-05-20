using WebApi.DTO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using iText.Kernel.Pdf;
using iText.Signatures;
using iText.Kernel.Geom;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Security;
using iText.Bouncycastle.X509;
using iText.Bouncycastle.Crypto;
using System.Security.Cryptography.X509Certificates;
using iText.Commons.Bouncycastle.Cert;
using Org.BouncyCastle.Crypto;
using System.IO;
using Org.BouncyCastle.Crypto.Digests;
using iText.Kernel.Crypto;

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
                        column.Item().Text($"Mã công ty: {dto.CompanyAccount}");
                        column.Item().Text($"Tên công ty: {dto.CompanyName}");
                        column.Item().Text($"Địa chỉ: {dto.CAddress}");
                        column.Item().Text($"Người đại diện: {dto.RootName}");
                        column.Item().Text($"Email: {dto.RootAccount}");
                        column.Item().Text($"Số điện thoại: {dto.RPhoneNumber}");

                        column.Item().PaddingVertical(15).Text("Điều khoản hợp đồng:").FontSize(14).Bold();
                        column.Item().Text("1. Hai bên đồng ý cung cấp và sử dụng dịch vụ theo điều kiện được nêu trong hợp đồng.");
                        column.Item().Text("2. Thời hạn hợp đồng: 12 tháng kể từ ngày ký.");
                        column.Item().Text("3. Các điều khoản khác theo quy định của pháp luật Việt Nam.");
                        column.Item().PaddingVertical(20).Text("Ngày lập hợp đồng: " + DateTime.Now.ToString("dd/MM/yyyy"));

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Đại diện Bên A").AlignCenter();
                            row.RelativeItem().Text("Đại diện Bên B").AlignCenter();
                        });

                        column.Item().Height(100); // khoảng trống để ký
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
            Rectangle rect = new Rectangle(100, 100, 200, 100);
            var appearance = signer.GetSignatureAppearance();
            // Set thông tin và định dạng chữ ký
            appearance
                .SetPageRect(rect)
                .SetPageNumber(1)
                .SetLocation("Hệ thống")
                .SetReason("Ký bởi Admin")
                .SetLayer2Text($"Ký bởi {staffId}\nNgày: {DateTime.Now:dd/MM/yyyy}")
                .SetRenderingMode(PdfSignatureAppearance.RenderingMode.DESCRIPTION);

            signer.SetFieldName("Signature1"); // field name mới được tạo nếu chưa có

            IExternalSignature externalSignature = new PrivateKeySignature(iPrivateKey, DigestAlgorithms.SHA256);
            IExternalDigest digest = new BouncyCastleDigest();

            signer.SignDetached(digest, externalSignature, chain.ToArray(), null, null, null, 0, PdfSigner.CryptoStandard.CADES);

            return signedPdfStream.ToArray();
        }
    }
}

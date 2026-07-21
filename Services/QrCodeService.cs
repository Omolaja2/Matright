using QRCoder;

namespace PharMarket.Services;

public class QrCodeService : IQrCodeService
{
    private readonly IWebHostEnvironment _env;

    public QrCodeService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public byte[] GenerateProductQrCode(int productId, string productName)
    {
        var payload = $"PHARPRODUCT:{productId}|{productName}";
        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var qrPng = new PngByteQRCode(qrData);
        return qrPng.GetGraphic(20);
    }

    public string SaveQrCode(byte[] qrBytes, int storeId, int productId)
    {
        var dir = Path.Combine(_env.WebRootPath, "uploads", "qrcodes", storeId.ToString());
        Directory.CreateDirectory(dir);

        var filePath = Path.Combine(dir, $"product_{productId}.png");
        File.WriteAllBytes(filePath, qrBytes);

        return $"/uploads/qrcodes/{storeId}/product_{productId}.png";
    }
}

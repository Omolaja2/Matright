namespace PharMarket.Services;

public interface IQrCodeService
{
    byte[] GenerateProductQrCode(int productId, string productName);
    string SaveQrCode(byte[] qrBytes, int storeId, int productId);
}

using System.Security.Cryptography;
using System.Text;

namespace PharMarket.Services;

public interface IPasswordEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}

public class PasswordEncryptionService : IPasswordEncryptionService
{
    private readonly byte[] _key;

    public PasswordEncryptionService(IConfiguration configuration)
    {
        var secret = configuration["Security:EncryptionKey"];
        if (string.IsNullOrEmpty(secret))
            secret = "PharMarket_SuperSecret_PasswordEncryptionKey_2026_!@#$%^&*()_+";
        _key = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
    }

    public string Encrypt(string plainText)
    {
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var nonce = new byte[12];
        RandomNumberGenerator.Fill(nonce);

        var cipherText = new byte[plainBytes.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(_key, tag.Length);
        aes.Encrypt(nonce, plainBytes, cipherText, tag);

        var result = new byte[nonce.Length + tag.Length + cipherText.Length];
        nonce.CopyTo(result, 0);
        tag.CopyTo(result, nonce.Length);
        cipherText.CopyTo(result, nonce.Length + tag.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        var data = Convert.FromBase64String(cipherText);

        var nonce = data.AsSpan(0, 12);
        var tag = data.AsSpan(12, 16);
        var cipherBytes = data.AsSpan(28);

        var plainBytes = new byte[cipherBytes.Length];

        using var aes = new AesGcm(_key, tag.Length);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
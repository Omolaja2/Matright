using Microsoft.AspNetCore.Http;

namespace PharMarket.Services;

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _env;

    public ImageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> UploadImageAsync(IFormFile file, int storeId)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        if (!allowed.Contains(ext))
            throw new InvalidOperationException("Only jpg, png, webp, gif images are allowed.");

        if (file.Length > 5 * 1024 * 1024)
            throw new InvalidOperationException("Image must be less than 5MB.");

        var dir = Path.Combine(_env.WebRootPath, "uploads", "products", storeId.ToString());
        Directory.CreateDirectory(dir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(dir, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/products/{storeId}/{fileName}";
    }

    public Task DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return Task.CompletedTask;
        var path = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    public string GetPlaceholderImage() => "/images/placeholder-product.png";
}

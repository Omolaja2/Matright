using Microsoft.AspNetCore.Http;

namespace PharMarket.Services;

public interface IImageService
{
    Task<string> UploadImageAsync(IFormFile file, int storeId);
    Task DeleteImageAsync(string imageUrl);
    string GetPlaceholderImage();
}

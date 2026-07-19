using PharMarket.Models.Entities;
using PharMarket.ViewModels.Products;

namespace PharMarket.Services;

public interface IProductService
{
    Task<ProductListViewModel> GetAllProductsAsync(int storeId, string? search, int? categoryId, int? supplierId, int page = 1, int pageSize = 20);
    Task<Product?> GetProductByIdAsync(int id, int storeId);
    Task<Product?> GetProductByBarcodeAsync(string barcode, int storeId);
    Task<Product> CreateProductAsync(ProductViewModel model, int storeId);
    Task UpdateProductAsync(ProductViewModel model, int storeId);
    Task DeleteProductAsync(int id, int storeId);
    Task<List<Product>> SearchProductsAsync(string query, int storeId);
}

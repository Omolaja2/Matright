using Microsoft.EntityFrameworkCore;
using PharMarket.Data;
using PharMarket.Exceptions;
using PharMarket.Models.Entities;
using PharMarket.ViewModels.Products;

namespace PharMarket.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProductListViewModel> GetAllProductsAsync(int storeId, string? search, int? categoryId, int? supplierId, int page = 1, int pageSize = 20)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Stock)
            .AsNoTracking()
            .Where(p => p.IsActive && p.StoreId == storeId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || (p.Barcode != null && p.Barcode.Contains(search)));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (supplierId.HasValue)
            query = query.Where(p => p.SupplierId == supplierId.Value);

        var totalCount = await query.CountAsync();
        var products = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListItem
            {
                Id = p.Id,
                Name = p.Name,
                Barcode = p.Barcode,
                ImageUrl = p.ImageUrl,
                CategoryName = p.Category.Name,
                SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                CostPrice = p.CostPrice,
                SalesPrice = p.SalesPrice,
                TotalStock = p.Stock != null ? p.Stock.StoreQuantity + p.Stock.ShelfQuantity : 0,
                IsActive = p.IsActive
            })
            .ToListAsync();

        return new ProductListViewModel
        {
            Products = products,
            SearchQuery = search,
            CategoryFilter = categoryId,
            SupplierFilter = supplierId,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            PageSize = pageSize
        };
    }

    public async Task<Product?> GetProductByIdAsync(int id, int storeId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Stock)
            .FirstOrDefaultAsync(p => p.Id == id && p.StoreId == storeId && p.IsActive);
    }

    public async Task<Product?> GetProductByBarcodeAsync(string barcode, int storeId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Stock)
            .FirstOrDefaultAsync(p => p.Barcode == barcode && p.StoreId == storeId && p.IsActive);
    }

    public async Task<Product> CreateProductAsync(ProductViewModel model, int storeId)
    {
        var product = new Product
        {
            Name = model.Name,
            Barcode = model.Barcode,
            Description = model.Description,
            CategoryId = model.CategoryId,
            SupplierId = model.SupplierId,
            CostPrice = model.CostPrice,
            SalesPrice = model.SalesPrice,
            TaxRate = model.TaxRate,
            MinimumStock = model.MinimumStock,
            ImageUrl = model.ImageUrl,
            StoreId = storeId,
            IsActive = model.IsActive
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _context.Stocks.Add(new Stock { ProductId = product.Id });
        await _context.SaveChangesAsync();

        return product;
    }

    public async Task UpdateProductAsync(ProductViewModel model, int storeId)
    {
        var product = await _context.Products.FindAsync(model.Id)
            ?? throw new NotFoundException("Product", model.Id);

        if (product.StoreId != storeId)
            throw new NotFoundException("Product", model.Id);

        product.Name = model.Name;
        product.Barcode = model.Barcode;
        product.Description = model.Description;
        product.CategoryId = model.CategoryId;
        product.SupplierId = model.SupplierId;
        product.CostPrice = model.CostPrice;
        product.SalesPrice = model.SalesPrice;
        product.TaxRate = model.TaxRate;
        product.MinimumStock = model.MinimumStock;
        product.ImageUrl = model.ImageUrl;
        product.IsActive = model.IsActive;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(int id, int storeId)
    {
        var product = await _context.Products.FindAsync(id)
            ?? throw new NotFoundException("Product", id);

        if (product.StoreId != storeId)
            throw new NotFoundException("Product", id);

        product.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    public async Task<List<Product>> SearchProductsAsync(string query, int storeId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Stock)
            .Where(p => p.IsActive && p.StoreId == storeId && (p.Name.Contains(query) || (p.Barcode != null && p.Barcode.Contains(query))))
            .OrderBy(p => p.Name)
            .Take(20)
            .ToListAsync();
    }
}

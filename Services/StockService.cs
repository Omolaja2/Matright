using Microsoft.EntityFrameworkCore;
using PharMarket.Data;
using PharMarket.Exceptions;
using PharMarket.Models.Entities;
using PharMarket.ViewModels.Stock;
using TransferDirection = PharMarket.ViewModels.Stock.TransferDirection;

namespace PharMarket.Services;

public class StockService : IStockService
{
    private readonly AppDbContext _context;

    public StockService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockViewModel>> GetAllStockAsync(int storeId)
    {
        return await GetStockQuery(storeId).ToListAsync();
    }

    public async Task<List<StockViewModel>> GetStoreStockAsync(int storeId)
    {
        return await GetStockQuery(storeId).Where(s => s.StoreQuantity > 0).ToListAsync();
    }

    public async Task<List<StockViewModel>> GetShelfStockAsync(int storeId)
    {
        return await GetStockQuery(storeId).Where(s => s.ShelfQuantity > 0).ToListAsync();
    }

    public async Task<List<StockViewModel>> GetLowStockAsync(int storeId)
    {
        return await GetStockQuery(storeId)
            .Where(s => s.IsLowStock)
            .ToListAsync();
    }

    public async Task<List<StockViewModel>> GetExpiringStockAsync(int storeId, int days = 30)
    {
        var cutoff = DateTime.UtcNow.AddDays(days);
        return await GetStockQuery(storeId)
            .Where(s => s.ExpirationDate.HasValue && s.ExpirationDate.Value <= cutoff && s.ExpirationDate.Value > DateTime.UtcNow)
            .OrderBy(s => s.ExpirationDate)
            .ToListAsync();
    }

    public async Task TransferStockAsync(TransferViewModel model, int storeId)
    {
        var stock = await _context.Stocks
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.ProductId == model.ProductId && s.Product.StoreId == storeId)
            ?? throw new NotFoundException("Stock for product", model.ProductId);

        if (model.Direction == TransferDirection.StoreToShelf)
        {
            if (stock.StoreQuantity < model.Quantity)
                throw new BadRequestException("Insufficient store stock for this transfer.");

            stock.StoreQuantity -= model.Quantity;
            stock.ShelfQuantity += model.Quantity;
        }
        else
        {
            if (stock.ShelfQuantity < model.Quantity)
                throw new BadRequestException("Insufficient shelf stock for this transfer.");

            stock.ShelfQuantity -= model.Quantity;
            stock.StoreQuantity += model.Quantity;
        }

        await _context.SaveChangesAsync();
    }

    public async Task AdjustStockAsync(int productId, int storeAdjustment, int shelfAdjustment, int storeId)
    {
        var stock = await _context.Stocks
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.Product.StoreId == storeId)
            ?? throw new NotFoundException("Stock for product", productId);

        stock.StoreQuantity = Math.Max(0, stock.StoreQuantity + storeAdjustment);
        stock.ShelfQuantity = Math.Max(0, stock.ShelfQuantity + shelfAdjustment);

        await _context.SaveChangesAsync();
    }

    private IQueryable<StockViewModel> GetStockQuery(int storeId)
    {
        return _context.Stocks
            .Include(s => s.Product)
                .ThenInclude(p => p.Category)
            .Where(s => s.Product.StoreId == storeId)
            .AsNoTracking()
            .Select(s => new StockViewModel
            {
                ProductId = s.ProductId,
                ProductName = s.Product.Name,
                Barcode = s.Product.Barcode,
                CategoryName = s.Product.Category.Name,
                StoreQuantity = s.StoreQuantity,
                ShelfQuantity = s.ShelfQuantity,
                ExpirationDate = s.ExpirationDate,
                IsLowStock = (s.StoreQuantity + s.ShelfQuantity) <= s.Product.MinimumStock,
                IsExpiringSoon = s.ExpirationDate.HasValue && s.ExpirationDate.Value <= DateTime.UtcNow.AddDays(30)
            });
    }
}

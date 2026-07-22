using Microsoft.EntityFrameworkCore;
using PharMarket.Data;
using PharMarket.Exceptions;
using PharMarket.Models.Entities;
using PharMarket.Helpers;
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

    public async Task<PagedResult<StockViewModel>> GetAllStockAsync(int storeId, int page = 1, int pageSize = 20)
    {
        var query = GetStockQuery(storeId);
        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResult<StockViewModel> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<PagedResult<StockViewModel>> GetStoreStockAsync(int storeId, int page = 1, int pageSize = 20)
    {
        var query = GetStockQuery(storeId).Where(s => s.StoreQuantity > 0);
        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResult<StockViewModel> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<PagedResult<StockViewModel>> GetShelfStockAsync(int storeId, int page = 1, int pageSize = 20)
    {
        var query = GetStockQuery(storeId).Where(s => s.ShelfQuantity > 0);
        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResult<StockViewModel> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<PagedResult<StockViewModel>> GetLowStockAsync(int storeId, int page = 1, int pageSize = 20)
    {
        var query = GetStockQuery(storeId).Where(s => s.IsLowStock);
        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResult<StockViewModel> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<PagedResult<StockViewModel>> GetExpiringStockAsync(int storeId, int days = 30, int page = 1, int pageSize = 20)
    {
        var cutoff = DateTime.UtcNow.AddDays(days);
        var query = GetStockQuery(storeId)
            .Where(s => s.ExpirationDate.HasValue && s.ExpirationDate.Value <= cutoff && s.ExpirationDate.Value > DateTime.UtcNow)
            .OrderBy(s => s.ExpirationDate);
        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResult<StockViewModel> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
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

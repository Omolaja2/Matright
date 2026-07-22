using PharMarket.Helpers;
using PharMarket.ViewModels.Stock;

namespace PharMarket.Services;

public interface IStockService
{
    Task<PagedResult<StockViewModel>> GetAllStockAsync(int storeId, int page = 1, int pageSize = 20);
    Task<PagedResult<StockViewModel>> GetStoreStockAsync(int storeId, int page = 1, int pageSize = 20);
    Task<PagedResult<StockViewModel>> GetShelfStockAsync(int storeId, int page = 1, int pageSize = 20);
    Task<PagedResult<StockViewModel>> GetLowStockAsync(int storeId, int page = 1, int pageSize = 20);
    Task<PagedResult<StockViewModel>> GetExpiringStockAsync(int storeId, int days = 30, int page = 1, int pageSize = 20);
    Task TransferStockAsync(TransferViewModel model, int storeId);
    Task AdjustStockAsync(int productId, int storeAdjustment, int shelfAdjustment, int storeId);
}

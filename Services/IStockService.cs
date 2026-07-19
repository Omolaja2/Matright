using PharMarket.ViewModels.Stock;

namespace PharMarket.Services;

public interface IStockService
{
    Task<List<StockViewModel>> GetAllStockAsync(int storeId);
    Task<List<StockViewModel>> GetStoreStockAsync(int storeId);
    Task<List<StockViewModel>> GetShelfStockAsync(int storeId);
    Task<List<StockViewModel>> GetLowStockAsync(int storeId);
    Task<List<StockViewModel>> GetExpiringStockAsync(int storeId, int days = 30);
    Task TransferStockAsync(TransferViewModel model, int storeId);
    Task AdjustStockAsync(int productId, int storeAdjustment, int shelfAdjustment, int storeId);
}

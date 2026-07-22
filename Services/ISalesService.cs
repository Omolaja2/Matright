using PharMarket.Models.Entities;
using PharMarket.ViewModels.POS;
using PharMarket.ViewModels.Sales;

namespace PharMarket.Services;

public interface ISalesService
{
    Task<Sale> ProcessSaleAsync(ProcessSaleViewModel model, int storeId, int? userId = null);
    Task<(SalesReportViewModel Model, int TotalCount)> GetSalesReportAsync(int storeId, DateTime? startDate, DateTime? endDate, Models.Enums.PaymentMethod? paymentMethod, int page = 1, int pageSize = 20);
    Task<SaleDetailsViewModel?> GetSaleDetailsAsync(int saleId, int storeId);
    Task<(List<Sale> Items, int TotalCount)> GetDailySalesSummaryAsync(DateTime date, int storeId, int page = 1, int pageSize = 20);
}

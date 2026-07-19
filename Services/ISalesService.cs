using PharMarket.Models.Entities;
using PharMarket.ViewModels.POS;
using PharMarket.ViewModels.Sales;

namespace PharMarket.Services;

public interface ISalesService
{
    Task<Sale> ProcessSaleAsync(ProcessSaleViewModel model, int storeId, int? userId = null);
    Task<SalesReportViewModel> GetSalesReportAsync(int storeId, DateTime? startDate, DateTime? endDate, Models.Enums.PaymentMethod? paymentMethod);
    Task<SaleDetailsViewModel?> GetSaleDetailsAsync(int saleId, int storeId);
    Task<List<Sale>> GetDailySalesSummaryAsync(DateTime date, int storeId);
}

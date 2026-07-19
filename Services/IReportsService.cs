using PharMarket.ViewModels.Dashboard;
using PharMarket.ViewModels.Reports;

namespace PharMarket.Services;

public interface IReportsService
{
    Task<DashboardViewModel> GetDashboardDataAsync(int storeId);
    Task<ProfitLossViewModel> GetProfitLossReportAsync(int storeId, DateTime? startDate, DateTime? endDate);
}

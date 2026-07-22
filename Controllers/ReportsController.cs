using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharMarket.Helpers;
using PharMarket.Models.Enums;
using PharMarket.Services;

namespace PharMarket.Controllers;

[Authorize]
public class ReportsController : BaseController
{
    private readonly IReportsService _reportsService;
    private readonly ISalesService _salesService;
    private readonly IFinanceService _financeService;

    public ReportsController(IReportsService reportsService, ISalesService salesService, IFinanceService financeService)
    {
        _reportsService = reportsService;
        _salesService = salesService;
        _financeService = financeService;
    }

    public async Task<IActionResult> Sales(DateTime? startDate, DateTime? endDate, PaymentMethod? paymentMethod, int page = 1)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var (model, totalCount) = await _salesService.GetSalesReportAsync(storeId.Value, startDate, endDate, paymentMethod, page);
        model.CurrentPage = page;
        model.TotalPages = (int)Math.Ceiling(totalCount / 20.0);
        return View(model);
    }

    public async Task<IActionResult> ProfitLoss(DateTime? startDate, DateTime? endDate)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var model = await _reportsService.GetProfitLossReportAsync(storeId.Value, startDate, endDate);
        return View(model);
    }

    public async Task<IActionResult> Expenses(DateTime? startDate, DateTime? endDate, string? category)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var (expenses, _) = await _financeService.GetAllExpensesAsync(storeId.Value, startDate, endDate, category, page: 1, pageSize: 100000);
        return View(expenses);
    }
}

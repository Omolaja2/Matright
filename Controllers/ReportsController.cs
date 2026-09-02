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

    public async Task<IActionResult> Sales(DateTime? startDate, DateTime? endDate, PaymentMethod? paymentMethod)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var model = await _salesService.GetSalesReportAsync(storeId.Value, startDate, endDate, paymentMethod);
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ProfitLoss(DateTime? startDate, DateTime? endDate)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var model = await _reportsService.GetProfitLossReportAsync(storeId.Value, startDate, endDate);
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Expenses(DateTime? startDate, DateTime? endDate, string? category)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var expenses = await _financeService.GetAllExpensesAsync(storeId.Value, startDate, endDate, category);
        return View(expenses);
    }
}

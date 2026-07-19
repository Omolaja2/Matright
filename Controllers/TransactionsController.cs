using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharMarket.Helpers;
using PharMarket.Services;

namespace PharMarket.Controllers;

[Authorize]
public class TransactionsController : BaseController
{
    private readonly IFinanceService _financeService;

    public TransactionsController(IFinanceService financeService)
    {
        _financeService = financeService;
    }

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var model = await _financeService.GetTransactionsAsync(storeId.Value, startDate, endDate);
        return View(model);
    }

    public async Task<IActionResult> CashAtHand()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var balance = await _financeService.GetCashAtHandAsync(storeId.Value);
        ViewBag.Balance = balance;
        return View();
    }
}

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

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int page = 1)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var (model, totalCount) = await _financeService.GetTransactionsAsync(storeId.Value, startDate, endDate, page);
        model.CurrentPage = page;
        model.TotalPages = (int)Math.Ceiling(totalCount / 20.0);
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

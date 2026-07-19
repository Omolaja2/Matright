using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharMarket.Data;
using PharMarket.Exceptions;
using PharMarket.Helpers;
using PharMarket.Services;
using PharMarket.ViewModels.POS;

namespace PharMarket.Controllers;

[Authorize]
public class SalesController : BaseController
{
    private readonly ISalesService _salesService;

    public SalesController(ISalesService salesService)
    {
        _salesService = salesService;
    }

    public IActionResult POS()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");
        return View(new POSViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProcessSaleViewModel model)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        if (!model.Items.Any())
        {
            SetErrorMessage("Cart is empty. Please add items before processing.");
            return RedirectToAction(nameof(POS));
        }

        var userId = User.GetUserId();
        var sale = await _salesService.ProcessSaleAsync(model, storeId.Value, userId);
        SetSuccessMessage($"Sale completed. Invoice: {sale.InvoiceNumber}");
        return RedirectToAction(nameof(Details), new { id = sale.Id });
    }

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, Models.Enums.PaymentMethod? paymentMethod)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var model = await _salesService.GetSalesReportAsync(storeId.Value, startDate, endDate, paymentMethod);
        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var model = await _salesService.GetSaleDetailsAsync(id, storeId.Value);
        if (model == null) throw new NotFoundException("Sale", id);
        return View(model);
    }

    public async Task<IActionResult> DailySummary(DateTime? date)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var targetDate = date ?? DateTime.UtcNow.Date;
        var sales = await _salesService.GetDailySalesSummaryAsync(targetDate, storeId.Value);
        ViewBag.Date = targetDate;
        return View(sales);
    }
}

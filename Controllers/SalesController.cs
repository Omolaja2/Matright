using System.Globalization;
using System.Text;
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

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, Models.Enums.PaymentMethod? paymentMethod, int page = 1)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var (model, totalCount) = await _salesService.GetSalesReportAsync(storeId.Value, startDate, endDate, paymentMethod, page);
        model.CurrentPage = page;
        model.TotalPages = (int)Math.Ceiling(totalCount / 20.0);
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

    public async Task<IActionResult> DailySummary(DateTime? date, int page = 1)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var targetDate = date ?? DateTime.UtcNow.Date;
        var (sales, totalCount) = await _salesService.GetDailySalesSummaryAsync(targetDate, storeId.Value, page);
        ViewBag.Date = targetDate;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / 20.0);
        ViewBag.TotalCount = totalCount;
        return View(sales);
    }

    [HttpGet]
    public async Task<IActionResult> Export(DateTime? startDate, DateTime? endDate, Models.Enums.PaymentMethod? paymentMethod)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var (model, _) = await _salesService.GetSalesReportAsync(storeId.Value, startDate, endDate, paymentMethod, page: 1, pageSize: 100000);

        var sb = new StringBuilder();
        sb.AppendLine("Invoice,Date,Items,Sub Total,Tax,Total,Payment Method,Cashier");

        foreach (var sale in model.Sales)
        {
            sb.AppendLine(string.Join(",",
                $"\"{sale.InvoiceNumber}\"",
                sale.SaleDate.ToString("dd MMM yyyy HH:mm"),
                sale.ItemCount,
                sale.SubTotal.ToString("F2", CultureInfo.InvariantCulture),
                sale.TaxAmount.ToString("F2", CultureInfo.InvariantCulture),
                sale.TotalAmount.ToString("F2", CultureInfo.InvariantCulture),
                sale.PaymentMethod,
                $"\"{sale.CashierName ?? "N/A"}\""
            ));
        }

        var fileName = $"sales_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", fileName);
    }
}

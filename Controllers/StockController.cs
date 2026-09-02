using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharMarket.Data;
using PharMarket.Helpers;
using PharMarket.Services;
using PharMarket.ViewModels.Stock;

namespace PharMarket.Controllers;

[Authorize]
public class StockController : BaseController
{
    private readonly IStockService _stockService;
    private readonly AppDbContext _context;

    public StockController(IStockService stockService, AppDbContext context)
    {
        _stockService = stockService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");
        var model = await _stockService.GetAllStockAsync(storeId.Value);
        return View(model);
    }

    public async Task<IActionResult> Store()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");
        var model = await _stockService.GetStoreStockAsync(storeId.Value);
        return View(model);
    }

    public async Task<IActionResult> Shelf()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");
        var model = await _stockService.GetShelfStockAsync(storeId.Value);
        return View(model);
    }

    public async Task<IActionResult> LowStock()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");
        var model = await _stockService.GetLowStockAsync(storeId.Value);
        return View(model);
    }

    public async Task<IActionResult> Expiring()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");
        var model = await _stockService.GetExpiringStockAsync(storeId.Value);
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Transfer()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");
        var model = new TransferViewModel
        {
            Products = new SelectList(_context.Products.Where(p => p.StoreId == storeId.Value).ToList(), "Id", "Name")
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Transfer(TransferViewModel model)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        if (!ModelState.IsValid)
        {
            model.Products = new SelectList(_context.Products.Where(p => p.StoreId == storeId.Value).ToList(), "Id", "Name");
            return View(model);
        }

        await _stockService.TransferStockAsync(model, storeId.Value);
        SetSuccessMessage("Stock transferred successfully.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Adjust(int productId, int storeAdjustment, int shelfAdjustment)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        await _stockService.AdjustStockAsync(productId, storeAdjustment, shelfAdjustment, storeId.Value);
        SetSuccessMessage("Stock adjusted successfully.");
        return RedirectToAction(nameof(Index));
    }
}

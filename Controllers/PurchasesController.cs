using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharMarket.Data;
using PharMarket.Exceptions;
using PharMarket.Helpers;
using PharMarket.Models.Entities;
using PharMarket.Models.Enums;
using PharMarket.ViewModels.Purchases;

namespace PharMarket.Controllers;

[Authorize]
public class PurchasesController : BaseController
{
    private readonly AppDbContext _context;

    public PurchasesController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var purchases = await _context.Purchases
            .Include(p => p.Supplier)
            .Where(p => !p.IsDeleted && p.StoreId == storeId.Value)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();
        return View(purchases);
    }

    private async Task<SelectList> GetSuppliersSelectList(int storeId) =>
        new(await _context.Suppliers.Where(s => s.StoreId == storeId && !s.IsDeleted).ToListAsync(), "Id", "Name");

    private async Task<SelectList> GetProductsSelectList(int storeId) =>
        new(await _context.Products.Where(p => p.IsActive && p.StoreId == storeId).ToListAsync(), "Id", "Name");

    public async Task<IActionResult> Create()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var model = new PurchaseViewModel
        {
            Suppliers = await GetSuppliersSelectList(storeId.Value)
        };
        ViewBag.Products = await GetProductsSelectList(storeId.Value);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PurchaseViewModel model)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        if (!ModelState.IsValid || !model.Items.Any())
        {
            model.Suppliers = await GetSuppliersSelectList(storeId.Value);
            ViewBag.Products = await GetProductsSelectList(storeId.Value);
            return View(model);
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var purchase = new Purchase
            {
                OrderNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                SupplierId = model.SupplierId,
                PurchaseDate = model.PurchaseDate,
                TotalAmount = model.TotalAmount,
                StoreId = storeId.Value,
                Status = PurchaseStatus.Pending,
                PurchaseItems = model.Items.Select(i => new PurchaseItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitCost = i.UnitCost,
                    ExpirationDate = i.ExpirationDate,
                    Total = i.Total
                }).ToList()
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            SetSuccessMessage("Purchase order created successfully.");
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var purchase = await _context.Purchases
            .Include(p => p.Supplier)
            .Include(p => p.PurchaseItems)
                .ThenInclude(pi => pi.Product)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted && p.StoreId == storeId.Value);

        if (purchase == null) throw new NotFoundException("Purchase", id);
        return View(purchase);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Receive(int id)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var purchase = await _context.Purchases
            .Include(p => p.PurchaseItems)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted && p.StoreId == storeId.Value)
            ?? throw new NotFoundException("Purchase", id);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in purchase.PurchaseItems)
            {
                var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.ProductId == item.ProductId);
                if (stock != null)
                {
                    stock.StoreQuantity += item.Quantity;
                    if (item.ExpirationDate.HasValue && (!stock.ExpirationDate.HasValue || item.ExpirationDate.Value < stock.ExpirationDate.Value))
                    {
                        stock.ExpirationDate = item.ExpirationDate;
                    }
                }
            }

            purchase.Status = PurchaseStatus.Received;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            SetSuccessMessage("Purchase received and stock updated.");
            return RedirectToAction(nameof(Details), new { id });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

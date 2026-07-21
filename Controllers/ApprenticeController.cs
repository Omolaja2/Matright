using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharMarket.Data;
using PharMarket.Exceptions;
using PharMarket.Helpers;
using PharMarket.Models.Entities;
using PharMarket.ViewModels.Apprentice;

namespace PharMarket.Controllers;

[Authorize]
public class ApprenticeController : BaseController
{
    private readonly AppDbContext _context;

    public ApprenticeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var query = _context.Products
            .Include(p => p.Stock)
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.StoreId == storeId.Value)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || (p.Barcode != null && p.Barcode.Contains(search)));

        var products = await query
            .OrderBy(p => p.Name)
            .Select(p => new ApprenticeProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Barcode = p.Barcode,
                CategoryName = p.Category.Name,
                CostPrice = p.CostPrice,
                SalesPrice = p.SalesPrice,
                StockLeft = p.Stock != null ? p.Stock.StoreQuantity + p.Stock.ShelfQuantity : 0,
                ExpirationDate = p.ExpirationDate,
                ImageUrl = p.ImageUrl
            })
            .ToListAsync();

        foreach (var p in products)
        {
            p.StockStatus = p.StockLeft <= 0 ? "OutOfStock" : p.StockLeft <= 10 ? "LowStock" : "InStock";
        }

        var userId = User.GetUserId();
        var todayStart = DateTime.UtcNow.Date;
        var todaySales = await _context.SaleItems
            .Include(si => si.Sale)
            .Include(si => si.Product)
            .Where(si => si.Sale.StoreId == storeId.Value
                && si.Sale.UserId == userId
                && si.Sale.SaleDate >= todayStart)
            .OrderByDescending(si => si.Sale.SaleDate)
            .Select(si => new ApprenticeSaleViewModel
            {
                ProductName = si.Product.Name,
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                Total = si.Total,
                SoldAt = si.Sale.SaleDate
            })
            .ToListAsync();

        var todayTotalSales = todaySales.Sum(s => s.Total);
        var todayTotalItems = todaySales.Sum(s => s.Quantity);

        var model = new ApprenticeDashboardViewModel
        {
            Products = products,
            TodaySales = todaySales,
            SearchQuery = search,
            TodayTotalSales = todayTotalSales,
            TodayTotalItems = todayTotalItems
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReduceStock(int productId, int quantity = 1)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return Json(new { success = false, message = "Store not found." });

        var userId = User.GetUserId();
        var userName = User.GetUserName();

        var store = await _context.Stores.FindAsync(storeId.Value);
        var storeName = store?.Name ?? "PharMarket";

        var stock = await _context.Stocks
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.Product.StoreId == storeId.Value);

        if (stock == null)
            return Json(new { success = false, message = "Product not found." });

        var totalAvailable = stock.StoreQuantity + stock.ShelfQuantity;
        if (totalAvailable < quantity)
            return Json(new { success = false, message = "Insufficient stock." });

        for (int i = 0; i < quantity; i++)
        {
            if (stock.ShelfQuantity > 0)
                stock.ShelfQuantity -= 1;
            else
                stock.StoreQuantity -= 1;
        }

        var sale = new Sale
        {
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6]}",
            SaleDate = DateTime.UtcNow,
            SubTotal = stock.Product.SalesPrice * quantity,
            TaxAmount = 0,
            TotalAmount = stock.Product.SalesPrice * quantity,
            PaymentMethod = Models.Enums.PaymentMethod.Cash,
            AmountPaid = stock.Product.SalesPrice * quantity,
            ChangeGiven = 0,
            CashierName = userName,
            StoreId = storeId.Value,
            UserId = userId
        };
        _context.Sales.Add(sale);

        var saleItem = new SaleItem
        {
            Sale = sale,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = stock.Product.SalesPrice,
            CostPrice = stock.Product.CostPrice,
            Total = stock.Product.SalesPrice * quantity
        };
        _context.SaleItems.Add(saleItem);

        await _context.SaveChangesAsync();

        var newTotal = stock.StoreQuantity + stock.ShelfQuantity;
        var status = newTotal <= 0 ? "OutOfStock" : newTotal <= stock.Product.MinimumStock ? "LowStock" : "InStock";

        return Json(new
        {
            success = true,
            newStock = newTotal,
            status,
            saleId = sale.Id,
            invoiceNumber = sale.InvoiceNumber,
            productName = stock.Product.Name,
            unitPrice = stock.Product.SalesPrice,
            total = sale.TotalAmount,
            soldAt = sale.SaleDate.ToString("dd MMM yyyy, hh:mm tt"),
            cashierName = userName,
            storeName = storeName,
            quantity = quantity
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetProductDetails(int id)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return Json(new { });

        var product = await _context.Products
            .Include(p => p.Stock)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id && p.StoreId == storeId.Value);

        if (product == null) return Json(new { });

        var stockLeft = product.Stock != null ? product.Stock.StoreQuantity + product.Stock.ShelfQuantity : 0;

        return Json(new
        {
            product.Id,
            product.Name,
            product.Description,
            product.Barcode,
            categoryName = product.Category?.Name,
            product.CostPrice,
            product.SalesPrice,
            stockLeft,
            product.ExpirationDate,
            product.ImageUrl,
            stockStatus = stockLeft <= 0 ? "OutOfStock" : stockLeft <= 10 ? "LowStock" : "InStock"
        });
    }

    [HttpGet]
    public async Task<IActionResult> MySales()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var userId = User.GetUserId();
        var todayStart = DateTime.UtcNow.Date;

        var sales = await _context.SaleItems
            .Include(si => si.Sale)
            .Include(si => si.Product)
            .Where(si => si.Sale.StoreId == storeId.Value
                && si.Sale.UserId == userId
                && si.Sale.SaleDate >= todayStart)
            .OrderByDescending(si => si.Sale.SaleDate)
            .Select(si => new ApprenticeSaleViewModel
            {
                ProductName = si.Product.Name,
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                Total = si.Total,
                SoldAt = si.Sale.SaleDate
            })
            .ToListAsync();

            return View(sales);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string q)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return Json(Array.Empty<object>());

        var products = await _context.Products
            .Include(p => p.Stock)
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.StoreId == storeId.Value && (p.Name.Contains(q) || (p.Barcode != null && p.Barcode.Contains(q))))
            .OrderBy(p => p.Name)
            .Take(20)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Barcode,
                categoryName = p.Category.Name,
                p.CostPrice,
                SalesPrice = p.SalesPrice,
                StockLeft = p.Stock != null ? p.Stock.StoreQuantity + p.Stock.ShelfQuantity : 0,
                p.ExpirationDate,
                p.ImageUrl
            })
            .ToListAsync();

        return Json(products);
    }
}

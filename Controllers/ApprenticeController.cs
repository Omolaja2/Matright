using System.Globalization;
using System.Text;
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

    public async Task<IActionResult> Index(string? search, int page = 1)
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

        var totalCount = await query.CountAsync();

        var products = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * 20)
            .Take(20)
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
        var yesterdayStart = todayStart.AddDays(-1);

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

        var yesterdayTotalSales = await _context.Sales
            .Where(s => s.StoreId == storeId.Value && s.UserId == userId
                && !s.IsDeleted && s.SaleDate >= yesterdayStart && s.SaleDate < todayStart)
            .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

        var yesterdayTotalItems = await _context.SaleItems
            .Where(si => si.Sale.StoreId == storeId.Value && si.Sale.UserId == userId
                && !si.Sale.IsDeleted && si.Sale.SaleDate >= yesterdayStart && si.Sale.SaleDate < todayStart)
            .SumAsync(si => (int?)si.Quantity) ?? 0;

        var todayTransactions = await _context.Sales
            .CountAsync(s => s.StoreId == storeId.Value && s.UserId == userId
                && !s.IsDeleted && s.SaleDate >= todayStart);

        var model = new ApprenticeDashboardViewModel
        {
            Products = products,
            TodaySales = todaySales,
            SearchQuery = search,
            TodayTotalSales = todayTotalSales,
            TodayTotalItems = todayTotalItems,
            YesterdayTotalSales = yesterdayTotalSales,
            YesterdayTotalItems = yesterdayTotalItems,
            TodayTransactions = todayTransactions,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / 20.0)
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ProcessCart([FromBody] CartSaleRequest request)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return Json(new { success = false, message = "Store not found." });

        if (request?.Items == null || !request.Items.Any())
            return Json(new { success = false, message = "Cart is empty." });

        var userId = User.GetUserId();
        var userName = User.GetUserName();
        var store = await _context.Stores.FindAsync(storeId.Value);
        var storeName = store?.Name ?? "PharMarket";

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            decimal subTotal = 0;
            var saleItems = new List<SaleItem>();

            foreach (var item in request.Items)
            {
                var stock = await _context.Stocks
                    .Include(s => s.Product)
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.Product.StoreId == storeId.Value);

                if (stock == null)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = $"Product not found." });
                }

                var totalAvailable = stock.StoreQuantity + stock.ShelfQuantity;
                if (totalAvailable < item.Quantity)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = $"Insufficient stock for {stock.Product.Name}. Only {totalAvailable} available." });
                }

                var toDeduct = item.Quantity;
                if (stock.ShelfQuantity >= toDeduct)
                    stock.ShelfQuantity -= toDeduct;
                else
                {
                    toDeduct -= stock.ShelfQuantity;
                    stock.ShelfQuantity = 0;
                    stock.StoreQuantity -= toDeduct;
                }

                var itemTotal = stock.Product.SalesPrice * item.Quantity;
                subTotal += itemTotal;

                saleItems.Add(new SaleItem
                {
                    SaleId = 0,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = stock.Product.SalesPrice,
                    CostPrice = stock.Product.CostPrice,
                    Total = itemTotal
                });
            }

            var sale = new Sale
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6]}",
                SaleDate = DateTime.UtcNow,
                SubTotal = subTotal,
                TaxAmount = 0,
                TotalAmount = subTotal,
                PaymentMethod = Models.Enums.PaymentMethod.Cash,
                AmountPaid = subTotal,
                ChangeGiven = 0,
                CashierName = userName,
                Notes = request.Notes,
                StoreId = storeId.Value,
                UserId = userId,
                SaleItems = saleItems
            };
            _context.Sales.Add(sale);

            foreach (var si in saleItems)
                si.SaleId = sale.Id;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var receiptItems = saleItems.Select(si => new
            {
                productName = _context.Products.Where(p => p.Id == si.ProductId).Select(p => p.Name).FirstOrDefault() ?? "",
                quantity = si.Quantity,
                unitPrice = si.UnitPrice,
                total = si.Total
            }).ToList();

            return Json(new
            {
                success = true,
                saleId = sale.Id,
                invoiceNumber = sale.InvoiceNumber,
                total = sale.TotalAmount,
                soldAt = sale.SaleDate.ToString("dd MMM yyyy, hh:mm tt"),
                cashierName = userName,
                storeName = storeName,
                notes = sale.Notes,
                items = receiptItems
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
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
    public async Task<IActionResult> MySales(int page = 1)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var userId = User.GetUserId();
        var todayStart = DateTime.UtcNow.Date;

        var query = _context.SaleItems
            .Include(si => si.Sale)
            .Include(si => si.Product)
            .Where(si => si.Sale.StoreId == storeId.Value
                && si.Sale.UserId == userId
                && si.Sale.SaleDate >= todayStart);

        var totalCount = await query.CountAsync();

        var sales = await query
            .OrderByDescending(si => si.Sale.SaleDate)
            .Skip((page - 1) * 20)
            .Take(20)
            .Select(si => new ApprenticeSaleViewModel
            {
                ProductName = si.Product.Name,
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                Total = si.Total,
                SoldAt = si.Sale.SaleDate
            })
            .ToListAsync();

        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / 20.0);
        ViewBag.TotalCount = totalCount;
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

public class CartSaleRequest
{
    public List<CartItemRequest> Items { get; set; } = new();
    public string? Notes { get; set; }
}

public class CartItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

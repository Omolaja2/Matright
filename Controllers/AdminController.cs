using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharMarket.Data;
using PharMarket.Helpers;
using PharMarket.ViewModels.Admin;

namespace PharMarket.Controllers;

[Authorize]
public class AdminController : BaseController
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> StaffSales(DateTime? date)
    {
        if (!User.IsAdmin()) return Forbid();

        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var selectedDate = date ?? DateTime.UtcNow.Date;
        var dayStart = selectedDate.Date;
        var dayEnd = dayStart.AddDays(1);

        var staffSales = await _context.Users
            .Where(u => u.StoreId == storeId.Value && u.Role == "Apprentice")
            .Select(u => new StaffSalesViewModel
            {
                StaffId = u.Id,
                StaffName = u.FullName,
                Position = u.Position,
                TotalItemsSold = _context.SaleItems
                    .Where(si => si.Sale.UserId == u.Id
                        && si.Sale.StoreId == storeId.Value
                        && si.Sale.SaleDate >= dayStart
                        && si.Sale.SaleDate < dayEnd)
                    .Sum(si => si.Quantity),
                TotalSalesAmount = _context.SaleItems
                    .Where(si => si.Sale.UserId == u.Id
                        && si.Sale.StoreId == storeId.Value
                        && si.Sale.SaleDate >= dayStart
                        && si.Sale.SaleDate < dayEnd)
                    .Sum(si => si.Total),
                SalesCount = _context.Sales
                    .Count(s => s.UserId == u.Id
                        && s.StoreId == storeId.Value
                        && s.SaleDate >= dayStart
                        && s.SaleDate < dayEnd),
                Sales = _context.SaleItems
                    .Include(si => si.Sale)
                    .Include(si => si.Product)
                    .Where(si => si.Sale.UserId == u.Id
                        && si.Sale.StoreId == storeId.Value
                        && si.Sale.SaleDate >= dayStart
                        && si.Sale.SaleDate < dayEnd)
                    .OrderByDescending(si => si.Sale.SaleDate)
                    .Select(si => new StaffSaleItemViewModel
                    {
                        SaleId = si.SaleId,
                        ProductName = si.Product.Name,
                        Quantity = si.Quantity,
                        UnitPrice = si.UnitPrice,
                        Total = si.Total,
                        SoldAt = si.Sale.SaleDate
                    })
                    .ToList()
            })
            .OrderByDescending(s => s.TotalSalesAmount)
            .ToListAsync();

        var model = new StaffSalesPageViewModel
        {
            SelectedDate = selectedDate,
            StaffSales = staffSales,
            GrandTotal = staffSales.Sum(s => s.TotalSalesAmount),
            GrandTotalItems = staffSales.Sum(s => s.TotalItemsSold)
        };

        return View(model);
    }
}

using Microsoft.EntityFrameworkCore;
using PharMarket.Data;
using PharMarket.Models.Enums;
using PharMarket.ViewModels.Dashboard;
using PharMarket.ViewModels.Reports;

namespace PharMarket.Services;

public class ReportsService : IReportsService
{
    private readonly AppDbContext _context;

    public ReportsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync(int storeId)
    {
        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-7);
        var monthStart = today.AddMonths(-1);

        var todaySales = await _context.Sales
            .Where(s => s.SaleDate.Date == today && !s.IsDeleted && s.StoreId == storeId)
            .SumAsync(s => s.TotalAmount);

        var weeklySales = await _context.Sales
            .Where(s => s.SaleDate >= weekStart && !s.IsDeleted && s.StoreId == storeId)
            .SumAsync(s => s.TotalAmount);

        var monthlySales = await _context.Sales
            .Where(s => s.SaleDate >= monthStart && !s.IsDeleted && s.StoreId == storeId)
            .SumAsync(s => s.TotalAmount);

        var todayExpenses = await _context.Expenses
            .Where(e => e.ExpenseDate.Date == today && !e.IsDeleted && e.StoreId == storeId)
            .SumAsync(e => e.Amount);

        var totalExpenses = await _context.Expenses
            .Where(e => e.ExpenseDate >= monthStart && !e.IsDeleted && e.StoreId == storeId)
            .SumAsync(e => e.Amount);

        var totalProducts = await _context.Products
            .CountAsync(p => p.IsActive && !p.IsDeleted && p.StoreId == storeId);

        var lowStockCount = await _context.Stocks
            .Include(s => s.Product)
            .CountAsync(s => s.Product.StoreId == storeId && (s.StoreQuantity + s.ShelfQuantity) <= s.Product.MinimumStock && s.Product.IsActive);

        var expiringCount = await _context.Stocks
            .Include(s => s.Product)
            .CountAsync(s => s.Product.StoreId == storeId && s.ExpirationDate.HasValue && s.ExpirationDate.Value <= DateTime.UtcNow.AddDays(30) && s.ExpirationDate.Value > DateTime.UtcNow);

        var unreadNotifications = await _context.Notifications
            .CountAsync(n => n.StoreId == storeId && !n.IsRead);

        var topSellingProducts = await _context.SaleItems
            .Include(si => si.Product)
            .Include(si => si.Sale)
            .Where(si => si.Sale.SaleDate >= monthStart && !si.Sale.IsDeleted && si.Sale.StoreId == storeId)
            .GroupBy(si => si.Product.Name)
            .Select(g => new TopSellingProduct
            {
                ProductName = g.Key,
                QuantitySold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Total)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(10)
            .ToListAsync();

        return new DashboardViewModel
        {
            TodaySales = todaySales,
            WeeklySales = weeklySales,
            MonthlySales = monthlySales,
            TodayExpenses = todayExpenses,
            TotalExpenses = totalExpenses,
            TotalProducts = totalProducts,
            LowStockCount = lowStockCount,
            ExpiringCount = expiringCount,
            UnreadNotificationCount = unreadNotifications,
            TopSellingProducts = topSellingProducts
        };
    }

    public async Task<ProfitLossViewModel> GetProfitLossReportAsync(int storeId, DateTime? startDate, DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        var sales = await _context.Sales
            .Include(s => s.SaleItems)
            .Where(s => s.SaleDate >= start && s.SaleDate <= end.AddDays(1) && !s.IsDeleted && s.StoreId == storeId)
            .ToListAsync();

        var totalRevenue = sales.Sum(s => s.TotalAmount);
        var costOfGoods = sales.SelectMany(s => s.SaleItems).Sum(si => si.CostPrice * si.Quantity);

        var totalExpenses = await _context.Expenses
            .Where(e => e.ExpenseDate >= start && e.ExpenseDate <= end.AddDays(1) && !e.IsDeleted && e.StoreId == storeId)
            .SumAsync(e => e.Amount);

        var expenseBreakdown = await _context.Expenses
            .Where(e => e.ExpenseDate >= start && e.ExpenseDate <= end.AddDays(1) && !e.IsDeleted && e.StoreId == storeId)
            .GroupBy(e => e.Category)
            .Select(g => new ExpenseCategorySummary
            {
                Category = g.Key,
                Amount = g.Sum(x => x.Amount),
                Percentage = totalExpenses > 0 ? (g.Sum(x => x.Amount) / totalExpenses) * 100 : 0
            })
            .OrderByDescending(x => x.Amount)
            .ToListAsync();

        return new ProfitLossViewModel
        {
            StartDate = start,
            EndDate = end,
            TotalRevenue = totalRevenue,
            CostOfGoods = costOfGoods,
            TotalExpenses = totalExpenses,
            ExpenseBreakdown = expenseBreakdown
        };
    }
}

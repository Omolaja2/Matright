using PharMarket.Models.Entities;

namespace PharMarket.ViewModels.Dashboard;

public class DashboardViewModel
{
    public decimal TodaySales { get; set; }
    public decimal WeeklySales { get; set; }
    public decimal MonthlySales { get; set; }
    public decimal TodayExpenses { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal CashAtHand { get; set; }
    public decimal TotalCapital { get; set; }
    public decimal ProfitLoss { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public int ExpiringCount { get; set; }
    public int OutOfStockCount { get; set; }
    public int UnreadNotificationCount { get; set; }
    public List<TopSellingProduct> TopSellingProducts { get; set; } = new();
    public List<SalesTrend> SalesTrend { get; set; } = new();
    public List<ExpenseBreakdown> ExpenseBreakdown { get; set; } = new();
}

public class TopSellingProduct
{
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public class SalesTrend
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class ExpenseBreakdown
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

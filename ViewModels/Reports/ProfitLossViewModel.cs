namespace PharMarket.ViewModels.Reports;

public class ProfitLossViewModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal CostOfGoods { get; set; }
    public decimal GrossProfit => TotalRevenue - CostOfGoods;
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit => GrossProfit - TotalExpenses;
    public decimal GrossMargin => TotalRevenue > 0 ? (GrossProfit / TotalRevenue) * 100 : 0;
    public decimal NetMargin => TotalRevenue > 0 ? (NetProfit / TotalRevenue) * 100 : 0;
    public List<ExpenseCategorySummary> ExpenseBreakdown { get; set; } = new();
    public List<DailyProfit> DailyProfits { get; set; } = new();
}

public class ExpenseCategorySummary
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class DailyProfit
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public decimal Profit => Revenue - Expenses;
}

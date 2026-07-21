namespace PharMarket.ViewModels.Apprentice;

public class ApprenticeDashboardViewModel
{
    public List<ApprenticeProductViewModel> Products { get; set; } = new();
    public List<ApprenticeSaleViewModel> TodaySales { get; set; } = new();
    public string? SearchQuery { get; set; }
    public decimal TodayTotalSales { get; set; }
    public int TodayTotalItems { get; set; }
}

public class ApprenticeProductViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public string? CategoryName { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalesPrice { get; set; }
    public int StockLeft { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? ImageUrl { get; set; }
    public string StockStatus { get; set; } = "InStock";
}

public class ApprenticeSaleViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
    public DateTime SoldAt { get; set; }
}

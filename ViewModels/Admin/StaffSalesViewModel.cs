namespace PharMarket.ViewModels.Admin;

public class StaffSalesPageViewModel
{
    public DateTime SelectedDate { get; set; }
    public List<StaffSalesViewModel> StaffSales { get; set; } = new();
    public decimal GrandTotal { get; set; }
    public int GrandTotalItems { get; set; }
}

public class StaffSalesViewModel
{
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string? Position { get; set; }
    public int TotalItemsSold { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public int SalesCount { get; set; }
    public List<StaffSaleItemViewModel> Sales { get; set; } = new();
}

public class StaffSaleItemViewModel
{
    public int SaleId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
    public DateTime SoldAt { get; set; }
}

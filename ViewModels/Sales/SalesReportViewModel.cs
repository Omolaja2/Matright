using PharMarket.Models.Enums;

namespace PharMarket.ViewModels.Sales;

public class SalesReportViewModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public PaymentMethod? PaymentMethodFilter { get; set; }
    public List<SalesReportItem> Sales { get; set; } = new();
    public decimal TotalSales => Sales.Sum(s => s.TotalAmount);
    public decimal TotalTax => Sales.Sum(s => s.TaxAmount);
    public int TotalTransactions => Sales.Count;
    public decimal AverageTransactionValue => TotalTransactions > 0 ? TotalSales / TotalTransactions : 0;
}

public class SalesReportItem
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? CashierName { get; set; }
    public int ItemCount { get; set; }
}

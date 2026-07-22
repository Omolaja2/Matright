using PharMarket.Models.Enums;

namespace PharMarket.ViewModels.Sales;

public class SaleDetailsViewModel
{
    public int SaleId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal ChangeGiven { get; set; }
    public string? CashierName { get; set; }
    public string? Notes { get; set; }
    public List<SaleItemDetail> Items { get; set; } = new();

    public List<SaleInsight> Insights { get; set; } = new();
}

public class SaleItemDetail
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal Total { get; set; }
}

public class SaleInsight
{
    public string Icon { get; set; } = "fas fa-lightbulb";
    public string Color { get; set; } = "var(--info)";
    public string Background { get; set; } = "#dbeafe";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

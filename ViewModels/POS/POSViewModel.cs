using System.ComponentModel.DataAnnotations;
using PharMarket.Models.Enums;

namespace PharMarket.ViewModels.POS;

public class POSViewModel
{
    public List<POSItem> CartItems { get; set; } = new();
    public decimal SubTotal => CartItems.Sum(i => i.Total);
    public decimal TaxAmount => CartItems.Sum(i => i.Total * i.TaxRate / 100);
    public decimal GrandTotal => SubTotal + TaxAmount;
}

public class POSItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal Total => UnitPrice * Quantity;
}

public class ProcessSaleViewModel
{
    [Required]
    public List<POSItem> Items { get; set; } = new();

    [Required]
    [Display(Name = "Payment Method")]
    public PaymentMethod PaymentMethod { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount paid must be greater than 0")]
    [Display(Name = "Amount Paid")]
    public decimal AmountPaid { get; set; }

    [StringLength(100)]
    [Display(Name = "Cashier Name")]
    public string? CashierName { get; set; }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharMarket.Models.Enums;

namespace PharMarket.ViewModels.Purchases;

public class PurchaseViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Supplier is required")]
    [Display(Name = "Supplier")]
    public int SupplierId { get; set; }

    [Required]
    [Display(Name = "Purchase Date")]
    public DateTime PurchaseDate { get; set; } = DateTime.Today;

    public List<PurchaseItemViewModel> Items { get; set; } = new();
    public decimal TotalAmount => Items.Sum(i => i.Total);

    public SelectList? Suppliers { get; set; }
}

public class PurchaseItemViewModel
{
    [Required]
    [Display(Name = "Product")]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    [Display(Name = "Quantity")]
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Unit cost must be greater than 0")]
    [Display(Name = "Unit Cost")]
    public decimal UnitCost { get; set; }

    [Display(Name = "Expiration Date")]
    public DateTime? ExpirationDate { get; set; }

    public decimal Total => Quantity * UnitCost;

    public SelectList? Products { get; set; }
}

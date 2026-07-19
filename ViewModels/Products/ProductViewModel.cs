using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharMarket.ViewModels.Products;

public class ProductViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200)]
    [Display(Name = "Product Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Barcode")]
    public string? Barcode { get; set; }

    [StringLength(1000)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Category is required")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Display(Name = "Supplier")]
    public int? SupplierId { get; set; }

    [Required(ErrorMessage = "Cost price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Cost price must be greater than 0")]
    [Display(Name = "Cost Price")]
    public decimal CostPrice { get; set; }

    [Required(ErrorMessage = "Sales price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Sales price must be greater than 0")]
    [Display(Name = "Sales Price")]
    public decimal SalesPrice { get; set; }

    [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100")]
    [Display(Name = "Tax Rate (%)")]
    public decimal TaxRate { get; set; } = 7.5m;

    [Range(0, int.MaxValue)]
    [Display(Name = "Minimum Stock")]
    public int MinimumStock { get; set; } = 10;

    public bool IsActive { get; set; } = true;

    [Display(Name = "Product Image URL")]
    public string? ImageUrl { get; set; }

    public SelectList? Categories { get; set; }
    public SelectList? Suppliers { get; set; }
}

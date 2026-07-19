using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharMarket.ViewModels.Stock;

public class StockViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int StoreQuantity { get; set; }
    public int ShelfQuantity { get; set; }
    public int TotalQuantity => StoreQuantity + ShelfQuantity;
    public DateTime? ExpirationDate { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsExpiringSoon { get; set; }
}

public class TransferViewModel
{
    [Required]
    [Display(Name = "Product")]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    [Display(Name = "Quantity")]
    public int Quantity { get; set; }

    [Required]
    [Display(Name = "Transfer Direction")]
    public TransferDirection Direction { get; set; }

    public SelectList? Products { get; set; }
}

public enum TransferDirection
{
    StoreToShelf = 0,
    ShelfToStore = 1
}

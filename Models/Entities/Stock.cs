using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharMarket.Models.Entities;

[Table("Stock")]
public class Stock : BaseEntity
{
    [Required]
    public int ProductId { get; set; }

    public int StoreQuantity { get; set; } = 0;

    public int ShelfQuantity { get; set; } = 0;

    public DateTime? ExpirationDate { get; set; }

    [ForeignKey("ProductId")]
    public Product Product { get; set; } = null!;

    [NotMapped]
    public int TotalQuantity => StoreQuantity + ShelfQuantity;

    [NotMapped]
    public bool IsLowStock => TotalQuantity <= Product?.MinimumStock;

    [NotMapped]
    public bool IsExpiringSoon => ExpirationDate.HasValue && ExpirationDate.Value <= DateTime.UtcNow.AddDays(30);

    [NotMapped]
    public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow;
}

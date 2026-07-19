using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharMarket.Models.Entities;

[Table("Products")]
public class Product : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Barcode { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public int? SupplierId { get; set; }

    [Required]
    public int StoreId { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CostPrice { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SalesPrice { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal TaxRate { get; set; } = 7.5m;

    public int MinimumStock { get; set; } = 10;

    public bool IsActive { get; set; } = true;

    [ForeignKey("StoreId")]
    public Store Store { get; set; } = null!;

    [ForeignKey("CategoryId")]
    public Category Category { get; set; } = null!;

    [ForeignKey("SupplierId")]
    public Supplier? Supplier { get; set; }

    public Stock? Stock { get; set; }
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();

    [NotMapped]
    public decimal ProfitMargin => SalesPrice > 0 ? ((SalesPrice - CostPrice) / SalesPrice) * 100 : 0;
}

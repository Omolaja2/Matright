using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PharMarket.Models.Enums;

namespace PharMarket.Models.Entities;

[Table("Purchases")]
public class Purchase : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    public int SupplierId { get; set; }

    [Required]
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Required]
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;

    [ForeignKey("SupplierId")]
    public Supplier Supplier { get; set; } = null!;

    [Required]
    public int StoreId { get; set; }

    [ForeignKey("StoreId")]
    public Store Store { get; set; } = null!;

    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
}

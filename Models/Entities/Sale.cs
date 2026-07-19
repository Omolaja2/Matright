using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PharMarket.Models.Enums;

namespace PharMarket.Models.Entities;

[Table("Sales")]
public class Sale : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ChangeGiven { get; set; }

    [StringLength(100)]
    public string? CashierName { get; set; }

    [Required]
    public int StoreId { get; set; }

    public int? UserId { get; set; }

    [ForeignKey("StoreId")]
    public Store Store { get; set; } = null!;

    [ForeignKey("UserId")]
    public User? User { get; set; }

    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}

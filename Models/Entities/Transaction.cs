using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PharMarket.Models.Enums;

namespace PharMarket.Models.Entities;

[Table("Transactions")]
public class Transaction : BaseEntity
{
    [Required]
    public TransactionType Type { get; set; }

    public int? ReferenceId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public TransactionDirection Direction { get; set; }

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal RunningBalance { get; set; }

    [Required]
    public int StoreId { get; set; }

    [ForeignKey("StoreId")]
    public Store Store { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PharMarket.Models.Enums;

namespace PharMarket.Models.Entities;

[Table("Expenses")]
public class Expense : BaseEntity
{
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [StringLength(500)]
    public string? Receipt { get; set; }

    [Required]
    public int StoreId { get; set; }

    [ForeignKey("StoreId")]
    public Store Store { get; set; } = null!;
}

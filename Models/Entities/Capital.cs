using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PharMarket.Models.Enums;

namespace PharMarket.Models.Entities;

[Table("Capital")]
public class Capital : BaseEntity
{
    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public CapitalType Type { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Required]
    public int StoreId { get; set; }

    [ForeignKey("StoreId")]
    public Store Store { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharMarket.Models.Entities;

[Table("TaxSettings")]
public class TaxSetting : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string TaxName { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal TaxRate { get; set; }

    public bool IsEnabled { get; set; } = true;

    [Required]
    public int StoreId { get; set; }

    [ForeignKey("StoreId")]
    public Store Store { get; set; } = null!;
}

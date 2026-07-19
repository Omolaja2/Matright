using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharMarket.Models.Entities;

[Table("Notifications")]
public class Notification : BaseEntity
{
    [Required]
    public int StoreId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = "info"; // info, warning, danger

    [Required]
    public bool IsRead { get; set; } = false;

    public int? ProductId { get; set; }

    [ForeignKey("StoreId")]
    public Store Store { get; set; } = null!;

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }
}

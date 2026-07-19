using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharMarket.Models.Entities;

[Table("Categories")]
public class Category : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public int StoreId { get; set; }

    [ForeignKey("StoreId")]
    public Store Store { get; set; } = null!;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

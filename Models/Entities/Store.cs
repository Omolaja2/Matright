using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharMarket.Models.Entities;

[Table("Stores")]
public class Store : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? OwnerName { get; set; }

    [StringLength(100)]
    public string? OwnerPosition { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(200)]
    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Capital> Capitals { get; set; } = new List<Capital>();
    public ICollection<TaxSetting> TaxSettings { get; set; } = new List<TaxSetting>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

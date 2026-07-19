using System.ComponentModel.DataAnnotations;

namespace PharMarket.ViewModels.Store;

public class StoreViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Store name is required")]
    [StringLength(200)]
    [Display(Name = "Store Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [StringLength(500)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Owner name is required")]
    [StringLength(100)]
    [Display(Name = "Owner / CEO Name")]
    public string OwnerName { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Position / Title")]
    public string? OwnerPosition { get; set; }

    [StringLength(20)]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [EmailAddress]
    [StringLength(200)]
    [Display(Name = "Email")]
    public string? Email { get; set; }
}

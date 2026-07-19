using System.ComponentModel.DataAnnotations;

namespace PharMarket.ViewModels.Suppliers;

public class SupplierViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Supplier name is required")]
    [StringLength(200)]
    [Display(Name = "Supplier Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "Contact Person")]
    public string? ContactPerson { get; set; }

    [StringLength(20)]
    [Phone(ErrorMessage = "Invalid phone number")]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [StringLength(200)]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(500)]
    [Display(Name = "Address")]
    public string? Address { get; set; }
}

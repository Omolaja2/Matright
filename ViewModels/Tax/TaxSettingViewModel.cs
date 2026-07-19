using System.ComponentModel.DataAnnotations;

namespace PharMarket.ViewModels.Tax;

public class TaxSettingViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Tax Name")]
    public string TaxName { get; set; } = string.Empty;

    [Required]
    [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100")]
    [Display(Name = "Tax Rate (%)")]
    public decimal TaxRate { get; set; }

    [Display(Name = "Enabled")]
    public bool IsEnabled { get; set; } = true;
}

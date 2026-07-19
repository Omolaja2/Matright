using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PharMarket.Models.Enums;

namespace PharMarket.ViewModels.Expenses;

public class ExpenseViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Category is required")]
    [StringLength(100)]
    [Display(Name = "Category")]
    public string Category { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Date")]
    public DateTime ExpenseDate { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "Payment Method")]
    public PaymentMethod PaymentMethod { get; set; }

    [StringLength(500)]
    [Display(Name = "Receipt Reference")]
    public string? Receipt { get; set; }

    public SelectList? Categories { get; set; }
}

public static class ExpenseCategories
{
    public static readonly string[] All = new[]
    {
        "Rent",
        "Utilities",
        "Salary",
        "Maintenance",
        "Transportation",
        "Marketing",
        "Insurance",
        "Supplies",
        "Other"
    };
}

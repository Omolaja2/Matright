using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharMarket.Exceptions;
using PharMarket.Helpers;
using PharMarket.Services;
using PharMarket.ViewModels.Expenses;

namespace PharMarket.Controllers;

[Authorize(Roles = "Admin")]
public class ExpensesController : BaseController
{
    private readonly IFinanceService _financeService;

    public ExpensesController(IFinanceService financeService)
    {
        _financeService = financeService;
    }

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string? category)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var expenses = await _financeService.GetAllExpensesAsync(storeId.Value, startDate, endDate, category);
        ViewBag.Categories = ExpenseCategories.All;
        return View(expenses);
    }

    public IActionResult Create()
    {
        ViewBag.Categories = ExpenseCategories.All;
        return View(new ExpenseViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExpenseViewModel model)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = ExpenseCategories.All;
            return View(model);
        }

        await _financeService.CreateExpenseAsync(model, storeId.Value);
        SetSuccessMessage("Expense recorded successfully.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var expenses = await _financeService.GetAllExpensesAsync(storeId.Value, null, null, null);
        var expense = expenses.FirstOrDefault(e => e.Id == id)
            ?? throw new NotFoundException("Expense", id);

        var model = new ExpenseViewModel
        {
            Id = expense.Id,
            Description = expense.Description,
            Amount = expense.Amount,
            Category = expense.Category,
            ExpenseDate = expense.ExpenseDate,
            PaymentMethod = expense.PaymentMethod,
            Receipt = expense.Receipt
        };

        ViewBag.Categories = ExpenseCategories.All;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ExpenseViewModel model)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = ExpenseCategories.All;
            return View(model);
        }

        await _financeService.UpdateExpenseAsync(model, storeId.Value);
        SetSuccessMessage("Expense updated successfully.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        await _financeService.DeleteExpenseAsync(id, storeId.Value);
        SetSuccessMessage("Expense deleted successfully.");
        return RedirectToAction(nameof(Index));
    }
}

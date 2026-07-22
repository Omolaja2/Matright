using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharMarket.Exceptions;
using PharMarket.Helpers;
using PharMarket.Services;
using PharMarket.ViewModels.Expenses;

namespace PharMarket.Controllers;

[Authorize]
public class ExpensesController : BaseController
{
    private readonly IFinanceService _financeService;

    public ExpensesController(IFinanceService financeService)
    {
        _financeService = financeService;
    }

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string? category, int page = 1)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var (expenses, totalCount) = await _financeService.GetAllExpensesAsync(storeId.Value, startDate, endDate, category, page);
        ViewBag.Categories = ExpenseCategories.All;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / 20.0);
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

        var (expenses, _) = await _financeService.GetAllExpensesAsync(storeId.Value, null, null, null, page: 1, pageSize: 100000);
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

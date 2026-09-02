using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharMarket.Data;
using PharMarket.Exceptions;
using PharMarket.Helpers;
using PharMarket.Models.Entities;
using PharMarket.ViewModels.Categories;

namespace PharMarket.Controllers;

[Authorize(Roles = "Admin")]
public class CategoriesController : BaseController
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var categories = await _context.Categories
            .Where(c => !c.IsDeleted && c.StoreId == storeId.Value)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(categories);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryViewModel model)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid category data.";
            return RedirectToAction(nameof(Index));
        }

        var category = new Category
        {
            Name = model.Name,
            Description = model.Description,
            StoreId = storeId.Value
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        SetSuccessMessage("Category created successfully.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryViewModel model)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid category data.";
            return RedirectToAction(nameof(Index));
        }

        var category = await _context.Categories.FindAsync(id) ?? throw new NotFoundException("Category", id);
        if (category.StoreId != storeId.Value) throw new NotFoundException("Category", id);

        category.Name = model.Name;
        category.Description = model.Description;

        await _context.SaveChangesAsync();
        SetSuccessMessage("Category updated successfully.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var category = await _context.Categories.FindAsync(id) ?? throw new NotFoundException("Category", id);
        if (category.StoreId != storeId.Value) throw new NotFoundException("Category", id);

        category.IsDeleted = true;
        await _context.SaveChangesAsync();
        SetSuccessMessage("Category deleted successfully.");
        return RedirectToAction(nameof(Index));
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharMarket.Data;
using PharMarket.Exceptions;
using PharMarket.Helpers;
using PharMarket.Models.Entities;
using PharMarket.ViewModels.Suppliers;

namespace PharMarket.Controllers;

[Authorize]
public class SuppliersController : BaseController
{
    private readonly AppDbContext _context;

    public SuppliersController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var query = _context.Suppliers
            .Where(s => !s.IsDeleted && s.StoreId == storeId.Value);

        var totalCount = await query.CountAsync();

        var suppliers = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * 20)
            .Take(20)
            .ToListAsync();

        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / 20.0);
        return View(suppliers);
    }

    public IActionResult Create()
    {
        return View(new SupplierViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierViewModel model)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        if (!ModelState.IsValid) return View(model);

        var supplier = new Supplier
        {
            Name = model.Name,
            ContactPerson = model.ContactPerson,
            Phone = model.Phone,
            Email = model.Email,
            Address = model.Address,
            StoreId = storeId.Value
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();
        SetSuccessMessage("Supplier added successfully.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var supplier = await _context.Suppliers.FindAsync(id) ?? throw new NotFoundException("Supplier", id);
        if (supplier.StoreId != storeId.Value) throw new NotFoundException("Supplier", id);

        var model = new SupplierViewModel
        {
            Id = supplier.Id,
            Name = supplier.Name,
            ContactPerson = supplier.ContactPerson,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SupplierViewModel model)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        if (!ModelState.IsValid) return View(model);

        var supplier = await _context.Suppliers.FindAsync(model.Id) ?? throw new NotFoundException("Supplier", model.Id);
        if (supplier.StoreId != storeId.Value) throw new NotFoundException("Supplier", model.Id);

        supplier.Name = model.Name;
        supplier.ContactPerson = model.ContactPerson;
        supplier.Phone = model.Phone;
        supplier.Email = model.Email;
        supplier.Address = model.Address;

        await _context.SaveChangesAsync();
        SetSuccessMessage("Supplier updated successfully.");
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id, int page = 1)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted && s.StoreId == storeId.Value)
            ?? throw new NotFoundException("Supplier", id);

        var purchasesQuery = _context.Purchases
            .Where(p => p.SupplierId == id && !p.IsDeleted);

        var totalCount = await purchasesQuery.CountAsync();

        var purchases = await purchasesQuery
            .OrderByDescending(p => p.PurchaseDate)
            .Skip((page - 1) * 20)
            .Take(20)
            .ToListAsync();

        supplier.Purchases = purchases;

        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / 20.0);
        return View(supplier);
    }
}

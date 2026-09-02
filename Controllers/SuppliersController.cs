using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharMarket.Data;
using PharMarket.Exceptions;
using PharMarket.Helpers;
using PharMarket.Models.Entities;
using PharMarket.ViewModels.Suppliers;

namespace PharMarket.Controllers;

[Authorize(Roles = "Admin")]
public class SuppliersController : BaseController
{
    private readonly AppDbContext _context;

    public SuppliersController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var suppliers = await _context.Suppliers
            .Where(s => !s.IsDeleted && s.StoreId == storeId.Value)
            .OrderBy(s => s.Name)
            .ToListAsync();
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

    public async Task<IActionResult> Details(int id)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var supplier = await _context.Suppliers
            .Include(s => s.Purchases)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted && s.StoreId == storeId.Value)
            ?? throw new NotFoundException("Supplier", id);
        return View(supplier);
    }
}

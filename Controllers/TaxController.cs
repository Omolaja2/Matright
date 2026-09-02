using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharMarket.Data;
using PharMarket.Exceptions;
using PharMarket.Helpers;
using PharMarket.Models.Entities;
using PharMarket.ViewModels.Tax;

namespace PharMarket.Controllers;

[Authorize(Roles = "Admin")]
public class TaxController : BaseController
{
    private readonly AppDbContext _context;

    public TaxController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        var settings = await _context.TaxSettings.Where(t => t.StoreId == storeId.Value).ToListAsync();
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(TaxSettingViewModel model)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return RedirectToAction("Setup", "Store");

        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Index));
        }

        var setting = await _context.TaxSettings.FindAsync(model.Id)
            ?? throw new NotFoundException("TaxSetting", model.Id);

        if (setting.StoreId != storeId.Value)
            throw new NotFoundException("TaxSetting", model.Id);

        setting.TaxName = model.TaxName;
        setting.TaxRate = model.TaxRate;
        setting.IsEnabled = model.IsEnabled;

        await _context.SaveChangesAsync();
        SetSuccessMessage("Tax settings updated successfully.");
        return RedirectToAction(nameof(Index));
    }
}

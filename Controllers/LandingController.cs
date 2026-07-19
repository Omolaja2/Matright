using Microsoft.AspNetCore.Mvc;
using PharMarket.Helpers;
using PharMarket.Services;

namespace PharMarket.Controllers;

public class LandingController : Controller
{
    private readonly IStoreService _storeService;

    public LandingController(IStoreService storeService)
    {
        _storeService = storeService;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.GetStoreId().HasValue)
                return RedirectToAction("Index", "Home");

            return RedirectToAction("Setup", "Store");
        }

        var storeExists = await _storeService.StoreExistsAsync();
        ViewBag.StoreExists = storeExists;
        return View();
    }
}

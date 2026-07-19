using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharMarket.Helpers;
using PharMarket.Services;

namespace PharMarket.Controllers;

[Authorize]
public class HomeController : BaseController
{
    private readonly IReportsService _reportsService;
    private readonly INotificationService _notificationService;

    public HomeController(IReportsService reportsService, INotificationService notificationService)
    {
        _reportsService = reportsService;
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Index()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue)
            return RedirectToAction("Setup", "Store");

        var dashboardData = await _reportsService.GetDashboardDataAsync(storeId.Value);
        ViewBag.UnreadNotificationCount = await _notificationService.GetUnreadCountAsync(storeId.Value);
        return View(dashboardData);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode = null)
    {
        var model = new Models.ErrorViewModel
        {
            RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = statusCode
        };
        return View(model);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharMarket.Helpers;
using PharMarket.Services;

namespace PharMarket.Controllers;

[Authorize]
public class NotificationsController : BaseController
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUnread()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return Json(Array.Empty<object>());

        var notifications = await _notificationService.GetUnreadNotificationsAsync(storeId.Value);
        return Json(notifications.Select(n => new
        {
            n.Id,
            n.Title,
            n.Message,
            n.Type,
            n.CreatedAt
        }));
    }

    [HttpPost]
    public async Task<IActionResult> MarkRead(int id)
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return Ok();

        await _notificationService.MarkAsReadAsync(id, storeId.Value);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllRead()
    {
        var storeId = User.GetStoreId();
        if (!storeId.HasValue) return Ok();

        await _notificationService.MarkAllAsReadAsync(storeId.Value);
        return Ok();
    }
}

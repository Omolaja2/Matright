using Microsoft.EntityFrameworkCore;
using PharMarket.Data;
using PharMarket.Models.Entities;

namespace PharMarket.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;

    public NotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Notification>> GetUnreadNotificationsAsync(int storeId)
    {
        return await _context.Notifications
            .Where(n => n.StoreId == storeId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Take(20)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int storeId)
    {
        return await _context.Notifications
            .CountAsync(n => n.StoreId == storeId && !n.IsRead);
    }

    public async Task MarkAsReadAsync(int notificationId, int storeId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.StoreId == storeId);

        if (notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(int storeId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.StoreId == storeId && !n.IsRead)
            .ToListAsync();

        foreach (var n in notifications)
            n.IsRead = true;

        await _context.SaveChangesAsync();
    }

    public async Task CreateNotificationAsync(int storeId, string title, string message, string type, int? productId = null)
    {
        var notification = new Notification
        {
            StoreId = storeId,
            Title = title,
            Message = message,
            Type = type,
            ProductId = productId
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }
}

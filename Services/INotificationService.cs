using PharMarket.Models.Entities;

namespace PharMarket.Services;

public interface INotificationService
{
    Task<List<Notification>> GetUnreadNotificationsAsync(int storeId);
    Task<int> GetUnreadCountAsync(int storeId);
    Task MarkAsReadAsync(int notificationId, int storeId);
    Task MarkAllAsReadAsync(int storeId);
    Task CreateNotificationAsync(int storeId, string title, string message, string type, int? productId = null);
}

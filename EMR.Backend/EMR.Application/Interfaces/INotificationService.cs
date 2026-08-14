using System.Collections.Generic;
using System.Threading.Tasks;
using EMR.Domain.Entities;

namespace EMR.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendToUserAsync(int userId, string title, string message);
        Task SendToRoleAsync(string role, string title, string message);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId, string role, bool unreadOnly = false);
        Task MarkAsReadAsync(int notificationId, int currentUserId);
        Task MarkAllAsReadAsync(int userId);
        Task DeleteNotificationAsync(int notificationId, int currentUserId);
        Task ClearAllForUserAsync(int userId);
    }
}

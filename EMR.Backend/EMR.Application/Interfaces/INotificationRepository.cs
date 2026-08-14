using System.Collections.Generic;
using System.Threading.Tasks;
using EMR.Domain.Entities;

namespace EMR.Application.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> AddAsync(Notification notification);
        Task<IEnumerable<Notification>> GetNotificationsAsync(int? userId, string? roleTarget, bool unreadOnly = false);
        Task<Notification?> GetByIdAsync(int id);
        Task UpdateAsync(Notification notification);
        Task DeleteAsync(Notification notification);
        Task DeleteAllForUserAsync(int userId);
        Task MarkAllAsReadAsync(int userId);
    }
}

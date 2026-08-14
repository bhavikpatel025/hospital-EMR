using System.Collections.Generic;
using System.Threading.Tasks;
using EMR.Application.Interfaces;
using EMR.Domain.Entities;
using EMR.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EMR.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            INotificationRepository notificationRepository,
            IHubContext<NotificationHub> hubContext)
        {
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
        }

        public async Task SendToUserAsync(int userId, string title, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message
            };

            await _notificationRepository.AddAsync(notification);

            // SignalR relies on mapping UserId to connection. Ensure authentication uses ClaimTypes.NameIdentifier = UserId
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification);
        }

        public async Task SendToRoleAsync(string role, string title, string message)
        {
            var notification = new Notification
            {
                RoleTarget = role,
                Title = title,
                Message = message
            };

            await _notificationRepository.AddAsync(notification);

            // Group name matches the Role
            await _hubContext.Clients.Group(role).SendAsync("ReceiveNotification", notification);
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId, string role, bool unreadOnly = false)
        {
            return await _notificationRepository.GetNotificationsAsync(userId, role, unreadOnly);
        }

        public async Task MarkAsReadAsync(int notificationId, int currentUserId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification != null && (notification.UserId == currentUserId || notification.UserId == null))
            {
                notification.IsRead = true;
                await _notificationRepository.UpdateAsync(notification);
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            await _notificationRepository.MarkAllAsReadAsync(userId);
        }

        public async Task DeleteNotificationAsync(int notificationId, int currentUserId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification != null && (notification.UserId == currentUserId || notification.UserId == null))
            {
                await _notificationRepository.DeleteAsync(notification);
            }
        }

        public async Task ClearAllForUserAsync(int userId)
        {
            await _notificationRepository.DeleteAllForUserAsync(userId);
        }
    }
}

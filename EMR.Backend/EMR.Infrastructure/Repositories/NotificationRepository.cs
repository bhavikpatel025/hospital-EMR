using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMR.Application.Interfaces;
using EMR.Domain.Entities;
using EMR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EMR.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Notification> AddAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<IEnumerable<Notification>> GetNotificationsAsync(int? userId, string? roleTarget, bool unreadOnly = false)
        {
            var query = _context.Notifications.AsQueryable();

            if (userId.HasValue && !string.IsNullOrEmpty(roleTarget))
            {
                query = query.Where(n => n.UserId == userId || n.RoleTarget == roleTarget);
            }
            else if (userId.HasValue)
            {
                query = query.Where(n => n.UserId == userId);
            }
            else if (!string.IsNullOrEmpty(roleTarget))
            {
                query = query.Where(n => n.RoleTarget == roleTarget);
            }

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(int id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task UpdateAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Notification notification)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllForUserAsync(int userId)
        {
            var notifications = await _context.Notifications.Where(n => n.UserId == userId).ToListAsync();
            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }
            
            if (notifications.Any())
            {
                _context.Notifications.UpdateRange(notifications);
                await _context.SaveChangesAsync();
            }
        }
    }
}

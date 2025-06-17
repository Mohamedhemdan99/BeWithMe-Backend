using Microsoft.AspNetCore.SignalR;
using BeWithMe.Data;
using BeWithMe.Hubs;
using BeWithMe.Models;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Cms;

namespace BeWithMe.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(string userId, string creatorId, string message, string type = "General");
        Task NotifyHelpersAsync(string eventname);
        Task NotifyUserAsync( string userId, object notificationData, string method);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
        Task MarkNotificationAsReadAsync(int notificationId);
        Task MarkAllNotificationsAsReadAsync(string userId);
        Task DeleteAllNotifications(string userId);
        Task DeleteNotification(int notificationId);



    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<Notification> CreateNotificationAsync(string creatorId, string recipientId, string content, string type)
        {
            var notification = new Notification
            {
                UserId = creatorId,
                RecipientId = recipientId,
                Content = content,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task NotifyHelpersAsync(string eventname="ReceiveNotification")
        {
            await _hubContext.Clients.Group("OnlineHelpers").SendAsync(eventname);
            
        }

        public async Task NotifyUserAsync( string userId, object notificationData, string method = "ReceiveNotification")
        {
            //var connections = await _hubContext.UserConnections.GetConnectionsAsync(userId);

            await _hubContext.Clients.User(userId).SendAsync(method, notificationData);

            //await _hubContext.Clients.Client("fbbxlnJ72J3Chl_322lb4w").SendAsync(method, notificationData);
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false)
        {
            var query = _context.Notifications.Where(n => n.RecipientId == userId);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            return await query.Include(p=>p.User)
                .OrderByDescending(n => n.CreatedAt)                
                .ToListAsync();
        }

        public async Task DeleteNotification(int notificationId)
        {
            // Find the notification by ID
            var notification = await _context.Notifications.FindAsync(notificationId);
            // Check if the notification exists

            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
            else
            {
                return; // Notification not found
            }
        }


        // Method to delete all notifications for a user
        public async Task DeleteAllNotifications(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();
            if (notifications == null || notifications.Count == 0)
            {
                return; // No notifications to delete
            }
            // Remove all notifications

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
        }

        public async Task MarkNotificationAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllNotificationsAsReadAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }
            await _context.SaveChangesAsync();
        }
    }
} 
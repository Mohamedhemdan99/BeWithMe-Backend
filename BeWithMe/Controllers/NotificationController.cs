using BeWithMe.Models;
using BeWithMe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BeWithMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Common")]
    [Authorize]
    public class NotificationController : ControllerBase
    {

        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        public NotificationController(INotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }
        /// <summary>

        [HttpGet]
        public async Task<IActionResult> GetUserNotifications([FromQuery] bool unreadOnly = false)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }
            

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);
            if (notifications == null || notifications.Count == 0)
            {
                return Ok(new { message = "No notifications found" });
            }


            //create notification data schema to send 
            var notificationData = notifications.Select(n => new
            {
                n.Id,
                n.Content,
                n.IsRead,
                n.CreatedAt,
                n.Type,
                PatientName = n.User.FullName,
                ProfileImageUrl = n.User.ProfileImageUrl

            }).ToList();

            return Ok(notificationData);
        }

        [HttpPut("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkNotificationAsReadAsync(id);
            return Ok(new { message = "Notification marked as read" });
        }

        // mark all notifications as read
        [HttpPut("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }
            await _notificationService.MarkAllNotificationsAsReadAsync(userId);
            return Ok(new { message = "All notifications marked as read" });
        }


        // delete all notifications
        [HttpDelete("delete-all")]
        public async Task<IActionResult> DeleteAllNotifications()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }
           await _notificationService.DeleteAllNotifications(userId);
            return Ok(new { message = "All notifications deleted" });
        }


        // delete a specific notification
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }
            await _notificationService.DeleteNotification(id);
            return Ok(new { message = "Notification deleted" });
        }


    }
}

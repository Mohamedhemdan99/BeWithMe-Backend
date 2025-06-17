using BeWithMe.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using BeWithMe.Models;
using BeWithMe.Migrations;

namespace BeWithMe.Hubs
{
    // لما المساعد يفتح التطبيق هندخله جروب اونلاين جروب عشان اعرف انه اونلاين وابعتله نوتفيكيشن

    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ApplicationDbContext _dbContext;

        public NotificationHub(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task JoinOnlineHelpersGroup()
            {
            var userId = Context.UserIdentifier; // Get user ID (ensure authentication is set up)
            await Groups.AddToGroupAsync(Context.ConnectionId, "OnlineHelpers");
            //add the connection to the user group in the database
            var group = _dbContext.hubGroups.FirstOrDefault(g => g.GroupName == "OnlineHelpers");
            if (group == null)
            {
                group = new hubGroup
                {
                    GroupId = Guid.NewGuid().ToString(),
                    GroupName = "OnlineHelpers"
                };
                _dbContext.hubGroups.Add(group);
                await _dbContext.SaveChangesAsync();
            }
            

            // Add user to the group if not already a member
            if (!_dbContext.hubUserGroups.Any(ug => ug.UserId == userId && ug.GroupId == group.GroupId))
            {
                _dbContext.hubUserGroups.Add(new hubUserGroup { UserId = userId, GroupId = group.GroupId });
                await _dbContext.SaveChangesAsync();
            }

            // Add all user's connections to the group
            var userConnections = _dbContext.hubUserConnections
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.ConnectionId)
                .ToList();
            // update the user status
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.Status = true;
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
            }
            foreach (var connId in userConnections)
            {
                await Groups.AddToGroupAsync(connId, group.GroupId);
            }

        }

        public async Task LeaveOnlineHelpersGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "OnlineHelpers");

            var userId = Context.UserIdentifier;

            // Remove user from the group
            var group = _dbContext.hubGroups.FirstOrDefault(g => g.GroupName == "OnlineHelpers");
            var userGroup = _dbContext.hubUserGroups
                .FirstOrDefault(ug => ug.UserId == userId && ug.GroupId == group.GroupId);

            if (userGroup != null)
            {
                _dbContext.hubUserGroups.Remove(userGroup);
                await _dbContext.SaveChangesAsync();
            }

            // Remove all user's connections from the group
            var userConnections = _dbContext.hubUserConnections
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.ConnectionId)
                .ToList();
            // update the user status 
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.Status = false;
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
            }
            foreach (var connId in userConnections)
            {
                await Groups.RemoveFromGroupAsync(connId, group.GroupId);
            }
        }

        public async Task JoinUserSpecificGroup(string userId)
        {
            // Use this method to join a user-specific group for direct notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                                               
        }

        public async Task LeaveUserSpecificGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        // This method is called when a new connection is established
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var userId = Context.UserIdentifier; // Get user ID (ensure authentication is set up)
            //var userId = "042dae20 - 49dc - 4d6e-8f10 - 7d9f5a71937d";

            _dbContext.hubUserConnections.Add(new hubUserConnections
            {
                UserId = userId,
                ConnectionId = connectionId,
            });
            try
            {

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving connection to database", ex);
            }
            // Add connection to all groups the user belongs to
            var userGroups = _dbContext.hubUserGroups
            .Where(ug => ug.UserId == userId)
            .Select(ug => ug.GroupId)
            .ToList();

            foreach (var groupId in userGroups)
            {
                await Groups.AddToGroupAsync(connectionId, groupId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;

            // Remove from UserConnection
            var userConnection = _dbContext.hubUserConnections.Find(connectionId);
            if (userConnection != null)
            {
                _dbContext.hubUserConnections.Remove(userConnection);
                await _dbContext.SaveChangesAsync();
            }
            await base.OnDisconnectedAsync(exception);
        }


    }
}


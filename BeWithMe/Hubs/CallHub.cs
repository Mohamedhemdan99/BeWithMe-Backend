using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BeWithMe.Hubs
{
    [Authorize]
    public class CallHub: Hub
    {       
        // This method is called when a user connects to the hub
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            // Add the user to a group based on their user ID
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            await base.OnConnectedAsync();
        }
        // This method is called when a user disconnects from the hub
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            // Remove the user from their group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            // Optionally, you can remove the connection ID from a database or in-memory store 
            await base.OnDisconnectedAsync(exception);
        }


        // 1. User A initiates a call to User B
        public async Task InitiateCall(string targetUserId)
        {
            var callerId = Context.UserIdentifier;

            // Notify User B of incoming call
            await Clients.User(targetUserId).SendAsync("IncomingCall", callerId);
        }

        // 2. User B accepts the call
        public async Task AcceptCall(string callerId)
        {
            var acceptorId = Context.UserIdentifier;

                // Notify User A that the call was accepted
                await Clients.User(callerId).SendAsync("CallAccepted", acceptorId);
            
        }

        // 3. Send text messages during the call
        public async Task SendPrivateVoiceCall(string targetUserId, string message)
        {
            //targetUserId = targetUserId ?? Context.UserIdentifier;
            await Clients.User(targetUserId).SendAsync("ReceiveVoiceCall", message);
        }

    }
}

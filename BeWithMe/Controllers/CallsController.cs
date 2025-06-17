#region Twilio Service
//using BeWithMe.Data;
//using BeWithMe.DTOs;
//using BeWithMe.Models;
//using BeWithMe.Models.Enums;
//using BeWithMe.Services;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace BeWithMe.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    [ApiExplorerSettings(GroupName = "Calls")]
//    public class CallsController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly INotificationService _notificationService;
//        private readonly IConfiguration config;
//        private readonly TimeSpan CALL_TIMEOUT = TimeSpan.FromMinutes(5); // Consider call abandoned after 5 minutes without heartbeat


//        public CallsController(ApplicationDbContext context,
//                                 UserManager<ApplicationUser> userManager,
//                                 INotificationService notificationService,
//                                 IConfiguration config)
//        {

//            _context = context;
//            _userManager = userManager;
//            _notificationService = notificationService;
//            this.config = config;
//        }


//        /// <summary>
//        /// initate a video call with the helper who accepted the post.
//        /// </summary>
//        /// <param name="AcceptorId"></param>
//        /// <param name="postId"></param>
//        /// <returns></returns>
//        [HttpPost("initiate-call")]
//        [Authorize(Roles = RoleConstants.Patient)]
//        public async Task<IActionResult> InitiateCall([FromBody] InitiateCallRequest request)
//        {
//            // Validate inputs  
//            if (string.IsNullOrEmpty(request.AcceptorId) || request.PostId <= 0)
//            {
//                return BadRequest("Invalid Acceptor ID or Post ID");
//            }

//            // Validate post exists and is accepted  
//            var post = await _context.Posts
//                .Include(p => p.Reactions)
//                .FirstOrDefaultAsync(p => p.Id == request.PostId);
//            if (post == null) return NotFound("Post not found");
//            if (post.Status != PostStatus.Accepted) return BadRequest("Only accepted posts can be confirmed");

//            // Validate user exists  
//            var user = await _context.Helpers.FindAsync(request.AcceptorId);
//            if (user == null) return NotFound("User not found");

//            var currentUserId = _userManager.GetUserId(User); // Helper method to extract UserId from claims
//            if (post.UserId != currentUserId) return Forbid("You are not authorized to initiate a call for this post");

//            // Ensure the acceptor has accepted the post
//            var hasAccepted = post.Reactions.Any(r => r.AcceptorId == request.AcceptorId);
//            if (!hasAccepted) return BadRequest("The specified user has not accepted this post");


//            // Check if there's already an active call for this post
//            var activeCall = await _context.CallHistories
//                .Where(c => c.PostId == request.PostId &&
//                          (c.Status == CallStatus.Initiated || c.Status == CallStatus.Connected))
//                .FirstOrDefaultAsync();

//            if (activeCall != null)
//            {
//                // Check if the call is stale (no heartbeat for a while)
//                if (DateTime.UtcNow - activeCall.LastHeartbeat > CALL_TIMEOUT)
//                {
//                    // Mark the stale call as disconnected
//                    activeCall.Status = CallStatus.Disconnected;
//                    activeCall.EndTime = DateTime.UtcNow;
//                    activeCall.DisconnectReason = "Call timed out due to inactivity";
//                    _context.CallHistories.Update(activeCall);
//                    await _context.SaveChangesAsync();
//                }
//                else
//                {
//                    return BadRequest("There is already an active call for this post");
//                }
//            }
//            // Generate Video Call Credentials
//            CallDetails callDetails;
//            GenerateAgoraCrediatilsService gvccs = new GenerateAgoraCrediatilsService(config);
//            try
//            {
//                callDetails = gvccs.GenerateVideoCallCredinatils(request.PostId);
//                if (callDetails == null) return StatusCode(500, "Failed to generate video call credentials");
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"An error occurred: {ex.Message}");
//            }
//            var callHistory = new CallHistory
//            {
//                PostId = request.PostId,
//                CallerId = currentUserId,
//                CalleeId = request.AcceptorId,
//                StartTime = DateTime.UtcNow,
//                LastHeartbeat = DateTime.UtcNow,
//                RoomName = callDetails.RoomName,
//                Status = CallStatus.Initiated
//            };
//            _context.CallHistories.Add(callHistory);
//            await _context.SaveChangesAsync();

//            // Notify the helper about the confirmation  
//            var notification = await _notificationService.CreateNotificationAsync(
//                request.AcceptorId,
//                "You have a new video call request",
//                NotificationType.VideoCall
//            );
//            var notificationData = new
//            {
//                Id = notification.Id,
//                PostId = request.PostId,
//                PatientName = post.User.FullName,
//                PatientId = post.UserId,
//                Content = $"You have a new video call",
//                CreatedAt = DateTime.UtcNow
//            };
//            // Corrected NotifyUserAsync call  
//            await _notificationService.NotifyUserAsync(request.AcceptorId, notificationData, "IncomingCall");

//            return Ok(new CallResponse
//            {
//                RoomName = callDetails.RoomName,
//                AccessToken = callDetails.AccessToken,
//                Message = "Call initiated successfully"
//            });
//        }

//        [HttpPost("join-call")]
//        [Authorize]
//        public async Task<IActionResult> JoinCallAsync([FromBody] JoinCallRequest request)
//        {
//            // Validate request
//            if (string.IsNullOrEmpty(request.RoomName))
//                return BadRequest("Room name is required");

//            // Extract postId from room name "room-{postId}"
//            if (!request.RoomName.StartsWith("room-") || !int.TryParse(request.RoomName.Split('-')[1], out int postId))
//                return BadRequest("Invalid room name format");

//            // Verify the call exists and is active
//            var callHistory = await _context.CallHistories
//                .FirstOrDefaultAsync(c => c.RoomName == request.RoomName);

//            if (callHistory == null)
//                return NotFound("Call not found");

//            // Check if call has already ended
//            if (callHistory.Status == CallStatus.Ended ||
//                callHistory.Status == CallStatus.Disconnected ||
//                callHistory.Status == CallStatus.Failed)
//                return BadRequest("This call has already ended");

//            var currentUserId = _userManager.GetUserId(User);
//            // Validate user is part of this call
//            if (currentUserId != callHistory.CallerId && currentUserId != callHistory.CalleeId)
//                return Forbid("You are not authorized to join this call");


//            GenerateAgoraCrediatilsService _generateCredsService = new GenerateAgoraCrediatilsService(config);
//            var callDetails = _generateCredsService.GenerateVideoCallCredinatils(postId);
//            // Update call status to connected when the call is joined
//            if (callHistory.Status == CallStatus.Initiated)
//            {
//                callHistory.Status = CallStatus.Connected;
//                callHistory.LastHeartbeat = DateTime.UtcNow;
//                _context.CallHistories.Update(callHistory);
//                await _context.SaveChangesAsync();

//                // Notify other participant that someone joined
//                var notifyUserId = (currentUserId == callHistory.CallerId) ? callHistory.CalleeId : callHistory.CallerId;
//                var notificationData = new
//                {
//                    RoomName = request.RoomName,
//                    Message = "The other participant has joined the call",
//                    Timestamp = DateTime.UtcNow
//                };

//                await _notificationService.NotifyUserAsync(notifyUserId, notificationData, "CallJoined");
//            }

//            return Ok(callDetails);
//        }


//        /// <summary>
//        /// Send heartbeat to keep the call active
//        /// </summary>
//        /// <param name="roomName">Name of the call room</param>
//        /// <returns>Status response</returns>
//        [HttpPost("heartbeat")]
//        [Authorize]
//        public async Task<IActionResult> SendHeartbeat([FromBody] HeartbeatRequest request)
//        {
//            if (string.IsNullOrEmpty(request.RoomName))
//                return BadRequest("Room name is required");

//            var callHistory = await _context.CallHistories
//                .FirstOrDefaultAsync(c => c.RoomName == request.RoomName);

//            if (callHistory == null)
//                return NotFound("Call not found");

//            var currentUserId = _userManager.GetUserId(User);

//            // Validate user is part of this call
//            if (currentUserId != callHistory.CallerId && currentUserId != callHistory.CalleeId)
//                return Forbid("You are not authorized to update this call");

//            // Only update heartbeat for active calls
//            if (callHistory.Status == CallStatus.Initiated || callHistory.Status == CallStatus.Connected)
//            {
//                callHistory.LastHeartbeat = DateTime.UtcNow;
//                _context.CallHistories.Update(callHistory);
//                await _context.SaveChangesAsync();

//                return Ok(new
//                {
//                    Message = "Heartbeat received",
//                    CallStatus = callHistory.Status.ToString()
//                });
//            }

//            return BadRequest(new
//            {
//                Message = "Call is not active",
//                CallStatus = callHistory.Status.ToString()
//            });
//        }


//        //[HttpPost("{roomId}/end")]
//        //[Authorize] // Ensure only authenticated users can end calls
//        //public async Task<IActionResult> EndCall(string roomId)
//        //{
//        //    // Validate roomId
//        //    if (string.IsNullOrEmpty(roomId))
//        //    {
//        //        return BadRequest("Invalid room ID");
//        //    }

//        //    // Find the call history entry for the given room
//        //    var callHistory = await _context.CallHistories
//        //        .FirstOrDefaultAsync(ch => ch.RoomName == roomId);

//        //    if (callHistory == null)
//        //    {
//        //        return NotFound("Call not found");
//        //    }

//        //    // Ensure the user is authorized to end the call
//        //    var currentUserId = _userManager.GetUserId(User); // Helper method to extract UserId from claims
//        //    if (callHistory.CallerId != currentUserId && callHistory.CalleeId != currentUserId)
//        //    {
//        //        return Forbid("You are not authorized to end this call");
//        //    }

//        //    // Update the call history with the end time
//        //    if (callHistory.EndTime == null) // Only update if the call hasn't already ended
//        //    {
//        //        callHistory.EndTime = DateTime.UtcNow;
//        //        await _context.SaveChangesAsync();
//        //    }

//        //    // update post status to completed
//        //    var post = await _context.Posts.FindAsync(callHistory.PostId);
//        //    if (post != null)
//        //    {
//        //        post.Status = PostStatus.Completed;
//        //        _context.Posts.Update(post);
//        //        await _context.SaveChangesAsync();
//        //    }

//        //    // Notify participants (optional)
//        //    var notificationData = new
//        //    {
//        //        RoomName = roomId,
//        //        Message = "The call has ended",
//        //        Timestamp = DateTime.UtcNow
//        //    };

//        //    // Notify the caller
//        //    await _notificationService.NotifyUserAsync(callHistory.CallerId.ToString(), notificationData, "CallEnded");

//        //    // Notify the callee
//        //    await _notificationService.NotifyUserAsync(callHistory.CalleeId.ToString(), notificationData, "CallEnded");

//        //    return Ok(new
//        //    {
//        //        Message = "Call ended successfully",
//        //        CallHistory = new
//        //        {
//        //            Id = callHistory.Id,
//        //            RoomName = callHistory.RoomName,
//        //            StartTime = callHistory.StartTime,
//        //            EndTime = callHistory.EndTime,
//        //            Duration = callHistory.EndTime.HasValue
//        //                ? (callHistory.EndTime.Value - callHistory.StartTime).TotalMinutes
//        //                :(double?) null
//        //        }
//        //    });
//        //}

//        /// <summary>
//        /// End an active call
//        /// </summary>
//        /// <param name="roomId">Room identifier for the call</param>
//        /// <returns>Call summary</returns>
//        [HttpPost("{roomId}/end")]
//        [Authorize]
//        public async Task<IActionResult> EndCall(string roomId)
//        {
//            // Validate roomId
//            if (string.IsNullOrEmpty(roomId))
//                return BadRequest("Invalid room ID");

//            // Find the call history entry for the given room
//            var callHistory = await _context.CallHistories
//                .FirstOrDefaultAsync(ch => ch.RoomName == roomId);

//            if (callHistory == null)
//                return NotFound("Call not found");

//            // Ensure the user is authorized to end the call
//            var currentUserId = _userManager.GetUserId(User);
//            if (callHistory.CallerId != currentUserId && callHistory.CalleeId != currentUserId)
//                return Forbid("You are not authorized to end this call");

//            // Only end calls that are active
//            if (callHistory.Status != CallStatus.Initiated && callHistory.Status != CallStatus.Connected)
//                return BadRequest($"Cannot end call in {callHistory.Status} state");

//            // Update the call history with the end time and status
//            callHistory.EndTime = DateTime.UtcNow;
//            callHistory.Status = CallStatus.Ended;
//            _context.CallHistories.Update(callHistory);

//            // Update post status to completed
//            var post = await _context.Posts.FindAsync(callHistory.PostId);
//            if (post != null)
//            {
//                post.Status = PostStatus.Completed;
//                _context.Posts.Update(post);
//            }

//            await _context.SaveChangesAsync();

//            // Notify participants
//            var notificationData = new
//            {
//                RoomName = roomId,
//                Message = "The call has ended",
//                Timestamp = DateTime.UtcNow
//            };

//            // Notify the caller (if not the one ending the call)
//            if (callHistory.CallerId != currentUserId)
//                await _notificationService.NotifyUserAsync(callHistory.CallerId, notificationData, "CallEnded");

//            // Notify the callee (if not the one ending the call)
//            if (callHistory.CalleeId != currentUserId)
//                await _notificationService.NotifyUserAsync(callHistory.CalleeId, notificationData, "CallEnded");

//            return Ok(new
//            {
//                Message = "Call ended successfully",
//                CallHistory = new
//                {
//                    Id = callHistory.Id,
//                    RoomName = callHistory.RoomName,
//                    StartTime = callHistory.StartTime,
//                    EndTime = callHistory.EndTime,
//                    Duration = callHistory.EndTime.HasValue
//                        ? (callHistory.EndTime.Value - callHistory.StartTime).TotalMinutes
//                        : (double?)null,
//                    Status = callHistory.Status.ToString()
//                }
//            });
//        }

//        /// <summary>
//        /// Clean up stale calls that weren't properly ended
//        /// </summary>
//        /// <returns>Summary of calls that were cleaned up</returns>
//        [HttpPost("cleanup-stale-calls")]
//        [Authorize(Roles = "Admin")] // Restrict to admin or use a background service
//        public async Task<IActionResult> CleanupStaleCalls()
//        {
//            var staleCalls = await _context.CallHistories
//                .Where(c => (c.Status == CallStatus.Initiated || c.Status == CallStatus.Connected) &&
//                          DateTime.UtcNow - c.LastHeartbeat > CALL_TIMEOUT)
//                .ToListAsync();

//            if (!staleCalls.Any())
//                return Ok(new { Message = "No stale calls found" });

//            foreach (var call in staleCalls)
//            {
//                call.Status = CallStatus.Disconnected;
//                call.EndTime = DateTime.UtcNow;
//                call.DisconnectReason = "Call automatically disconnected due to inactivity";

//                // Update corresponding post to completed if it was in Accepted state
//                var post = await _context.Posts.FindAsync(call.PostId);
//                if (post != null && post.Status == PostStatus.Accepted)
//                {
//                    post.Status = PostStatus.Completed;
//                    _context.Posts.Update(post);
//                }
//            }

//            _context.CallHistories.UpdateRange(staleCalls);
//            await _context.SaveChangesAsync();

//            // Notify users about disconnected calls
//            foreach (var call in staleCalls)
//            {
//                var notificationData = new
//                {
//                    RoomName = call.RoomName,
//                    Message = "The call was disconnected due to inactivity",
//                    Timestamp = DateTime.UtcNow
//                };

//                await _notificationService.NotifyUserAsync(call.CallerId, notificationData, "CallDisconnected");
//                await _notificationService.NotifyUserAsync(call.CalleeId, notificationData, "CallDisconnected");
//            }

//            return Ok(new
//            {
//                Message = $"Cleaned up {staleCalls.Count} stale calls",
//                CleanedCallIds = staleCalls.Select(c => c.Id).ToList()
//            });
//        }
//        /// <summary>
//        /// Get call history for a user
//        /// </summary>
//        /// <param name="userId">User identifier</param>
//        /// <returns>List of call history records</returns>
//        [HttpGet("{userId}/call-history")]
//        public async Task<IActionResult> GetUserCallHistory(string userId)
//        {
//            var callHistory = await _context.CallHistories
//                .Where(ch => ch.CallerId == userId || ch.CalleeId == userId)
//                .OrderByDescending(ch => ch.StartTime)
//                .Select(ch => new CallHistoryDto
//                {
//                    Id = ch.Id,
//                    PostId = ch.PostId,

//                    RoomName = ch.RoomName,
//                    Caller = new UserDto
//                    {
//                        Id = ch.Caller.Id,
//                        FullName = ch.Caller.FullName,
//                        username = ch.Caller.UserName,
//                        PictureUrl = ch.Caller.ProfileImageUrl,
//                        status = ch.Caller.Status,

//                    },
//                    Callee = new UserDto
//                    {
//                        Id = ch.Callee.Id,
//                        FullName = ch.Callee.FullName,
//                        username = ch.Caller.UserName,
//                        PictureUrl = ch.Callee.ProfileImageUrl,
//                        status = ch.Callee.Status
//                    },
//                    StartTime = ch.StartTime,
//                    EndTime = ch.EndTime,
//                    Status = ch.Status.ToString(),
//                    Duration = ch.EndTime.HasValue ? (ch.EndTime.Value - ch.StartTime).TotalMinutes : (double?)null
//                })
//                .ToListAsync();

//            return Ok(callHistory);
//        }

//        // delete call history
//        [HttpDelete("{callId}")]
//        public async Task<IActionResult> DeleteCallHistory(int callId)
//        {
//            var callHistory = await _context.CallHistories.FindAsync(callId);
//            if (callHistory == null)
//            {
//                return NotFound("Call history not found");
//            }
//            // Ensure the user is authorized to delete this call record
//            var currentUserId = _userManager.GetUserId(User);
//            if (callHistory.CallerId != currentUserId && callHistory.CalleeId != currentUserId)
//                return Forbid("You are not authorized to delete this call record");

//            _context.CallHistories.Remove(callHistory);
//            await _context.SaveChangesAsync();
//            return Ok("Call history deleted successfully");
//        }
//    }
//    public class HeartbeatRequest
//    {
//        public string RoomName { get; set; }
//    }
//}

#endregion



using BeWithMe.Data;
using BeWithMe.DTOs;
using BeWithMe.Models;
using BeWithMe.Models.Enums;
using BeWithMe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeWithMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Calls")]
    public class CallsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration config;
        private readonly TimeSpan CALL_TIMEOUT = TimeSpan.FromMinutes(5);
        private readonly ILogger<CallsController> _logger;

        public CallsController(ApplicationDbContext context,
                               UserManager<ApplicationUser> userManager,
                               INotificationService notificationService,
                               IConfiguration config,
                               ILogger<CallsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
            this.config = config;
            _logger = logger;
        }

        [HttpPost("initiate-call")]
        [Authorize(Roles = RoleConstants.Patient)]
        public async Task<IActionResult> InitiateCall([FromBody] InitiateCallRequest request)
        {
            if (string.IsNullOrEmpty(request.AcceptorId) || request.PostId <= 0)
            {
                return BadRequest("Invalid Acceptor ID or Post ID");
            }

            // Create transaction with execution strategy to handle transient failures
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync<IActionResult>(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var post = await _context.Posts
                        .Include(p => p.Reactions)
                        .Include(p => p.User)
                        .FirstOrDefaultAsync(p => p.Id == request.PostId);
                    if (post == null) return NotFound("Post not found");
                    //if (post.Status != PostStatus.Accepted) return BadRequest("Only accepted posts can be confirmed");
                    
                    var user = await _context.Helpers.FindAsync(request.AcceptorId);
                    if (user == null) return NotFound("User not found");

                    var currentUserId = _userManager.GetUserId(User);
                    if (post.UserId != currentUserId) return Forbid("You are not authorized to initiate a call for this post");

                    var hasAccepted = post.Reactions.Any(r => r.AcceptorId == request.AcceptorId);
                    if (!hasAccepted) return BadRequest("The specified user has not accepted this post");

                    var activeCall = await _context.CallHistories
                        .Where(c => c.PostId == request.PostId &&
                                    (c.Status == CallStatus.Initiated || c.Status == CallStatus.Connected))
                        .FirstOrDefaultAsync();

                    if (activeCall != null)
                    {
                        if (DateTime.UtcNow - activeCall.LastHeartbeat > CALL_TIMEOUT)
                        {
                            activeCall.Status = CallStatus.Disconnected;
                            activeCall.EndTime = DateTime.UtcNow;
                            activeCall.DisconnectReason = "Call timed out due to inactivity";
                            _context.CallHistories.Update(activeCall);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            return BadRequest("There is already an active call for this post");
                        }
                    }
                    string AppId = config["Agora:AppId"];
                    //string appCertificate = config["Agora:AppCertificate"];
                    var g6d = new Generate6Digits();
                    //var token = gacs.BuildToken(AppId, appCertificate,$"room-{post.Id}",1234567890,GenerateAgoraCredentialsService.TokenType.Rtc,3600);
                    var Uid = g6d.GenerateSixDigitCode();  
                    if (Uid == null) return StatusCode(500, "Failed to generate video call credentials");

                    var callHistory = new CallHistory
                    {   
                        PostId = request.PostId,
                        CallerId = currentUserId,
                        CalleeId = request.AcceptorId,
                        StartTime = DateTime.UtcNow,
                        LastHeartbeat = DateTime.UtcNow,
                        RoomName = $"room-{post.Id}", // Stores Agora ChannelName
                        Status = CallStatus.Initiated
                    };
                    _context.CallHistories.Add(callHistory);
                    await _context.SaveChangesAsync();

                    //retreive the call history with sorted by start time as descending
                    var call = await _context.CallHistories
                        .Where(c => c.PostId == request.PostId)
                        .OrderByDescending(c => c.StartTime)
                        .FirstOrDefaultAsync();


                    var notification = await _notificationService.CreateNotificationAsync(
                        request.AcceptorId,
                        "You have a new video call request",
                        NotificationType.VideoCall
                    );
                    var notificationData = new
                    {
                        Id = notification.Id,
                        PostId = request.PostId,
                        PatientName = post.User.FullName ?? "unknown",
                        PatientId = post.UserId,
                        imageURL = post.User.ProfileImageUrl,
                        AppId = AppId,
                        ChannelName = $"room-{post.Id}",
                        Uid = Uid,
                        Content = "You have a new video call",
                    
                    };
                    await _notificationService.NotifyUserAsync(request.AcceptorId, notificationData, "IncomingCall");

                    // Commit the transaction if everything is successful
                    await transaction.CommitAsync();

                    return Ok(new CallResponse
                    {
                        AppId = AppId,
                        Uid = Uid,
                        ChannelName = $"room-{post.Id}",
                        Message = "Call initiated successfully"
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Call initiation error");
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Call initiation failed due to an internal error" });
                }
            });
        }
        [HttpPost("join-call")]
        [Authorize]
        public async Task<IActionResult> JoinCallAsync([FromBody] JoinCallRequest request)
        {
            if (string.IsNullOrEmpty(request.RoomName))
                return BadRequest("Room name is required");

            if (!request.RoomName.StartsWith("room-") || !int.TryParse(request.RoomName.Split('-')[1], out int postId))
                return BadRequest("Invalid room name format");

            var callHistory = await _context.CallHistories
               .Where(c => c.RoomName == request.RoomName)
                .OrderByDescending(c => c.StartTime)
                        .FirstOrDefaultAsync();
            

            if (callHistory == null)
                return NotFound("Call not found");

            if (callHistory.Status == CallStatus.Ended ||
                callHistory.Status == CallStatus.Disconnected ||
                callHistory.Status == CallStatus.Failed)
                return BadRequest("This call has already ended");

            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId != callHistory.CallerId && currentUserId != callHistory.CalleeId)
                return Forbid("You are not authorized to join this call");

            //var g6d = new Generate6Digits();
            //var uid = g6d.GenerateSixDigitCode();

            if (callHistory.Status == CallStatus.Initiated)
            {
                callHistory.Status = CallStatus.Connected;
                callHistory.LastHeartbeat = DateTime.UtcNow;
                _context.CallHistories.Update(callHistory);
                await _context.SaveChangesAsync();

                var notifyUserId = (currentUserId == callHistory.CallerId) ? callHistory.CalleeId : callHistory.CallerId;
                var notificationData = new
                {
                    ChannelName = request.RoomName,
                    Message = "The other participant has joined the call",
                    Timestamp = DateTime.UtcNow
                };
                await _notificationService.NotifyUserAsync(notifyUserId, notificationData, "CallJoined");
            }
            var AppId = config["Agora:AppId"];
            return Ok(new CallResponse
            {
                AppId = request.AppId,
                Uid = request.Uid,
                ChannelName = request.RoomName,
                Message = "Joined call successfully"
            });
        }

        [HttpPost("heartbeat")]
        [Authorize]
        public async Task<IActionResult> SendHeartbeat([FromBody] HeartbeatRequest request)
        {
            if (string.IsNullOrEmpty(request.RoomName))
                return BadRequest("Room name is required");

            var callHistory = await _context.CallHistories
                .FirstOrDefaultAsync(c => c.RoomName == request.RoomName);

            if (callHistory == null)
                return NotFound("Call not found");

            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId != callHistory.CallerId && currentUserId != callHistory.CalleeId)
                return Forbid("You are not authorized to update this call");

            if (callHistory.Status == CallStatus.Initiated || callHistory.Status == CallStatus.Connected)
            {
                callHistory.LastHeartbeat = DateTime.UtcNow;
                _context.CallHistories.Update(callHistory);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Heartbeat received",
                    CallStatus = callHistory.Status.ToString()
                });
            }

            return BadRequest(new
            {
                Message = "Call is not active",
                CallStatus = callHistory.Status.ToString()
            });
        }

        [HttpPost("{roomId}/end")]
        [Authorize]
        public async Task<IActionResult> EndCall(string roomId)
        {
            if (string.IsNullOrEmpty(roomId))
                return BadRequest("Invalid room ID");

            var callHistory = await _context.CallHistories
               .Where(c => c.RoomName == roomId)
                .OrderByDescending(c => c.StartTime)
                        .FirstOrDefaultAsync();
            if (callHistory == null)
                return NotFound("Call not found");

            var currentUserId = _userManager.GetUserId(User);
            if (callHistory.CallerId != currentUserId && callHistory.CalleeId != currentUserId)
                return Forbid("You are not authorized to end this call");

            //if (callHistory.Status != CallStatus.Initiated && callHistory.Status != CallStatus.Connected)
            //    return BadRequest(new { message = $"Cannot end call in {callHistory.Status} state" ,status = callHistory.Status });

            callHistory.EndTime = DateTime.UtcNow;
            callHistory.Status = CallStatus.Ended;
            _context.CallHistories.Update(callHistory);

            var post = await _context.Posts.FindAsync(callHistory.PostId);
            if (post != null)
            {
                post.Status = PostStatus.Completed;
                _context.Posts.Update(post);
            }

            await _context.SaveChangesAsync();

            var notificationData = new
            {
                ChannelName = roomId,
                Message = "The call has ended",
                Timestamp = DateTime.UtcNow
            };

            if (callHistory.CallerId != currentUserId)
                await _notificationService.NotifyUserAsync(callHistory.CallerId, notificationData, "CallEnded");

            if (callHistory.CalleeId != currentUserId)
                await _notificationService.NotifyUserAsync(callHistory.CalleeId, notificationData, "CallEnded");

            return Ok(new
            {
                Message = "Call ended successfully",
                CallHistory = new
                {
                    Id = callHistory.Id,
                    RoomName = callHistory.RoomName,
                    StartTime = callHistory.StartTime,
                    EndTime = callHistory.EndTime,
                    Duration = callHistory.EndTime.HasValue
                        ? (callHistory.EndTime.Value - callHistory.StartTime).TotalMinutes
                        : (double?)null,
                    Status = callHistory.Status.ToString()
                }
            });
        }

        [HttpPost("cleanup-stale-calls")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> CleanupStaleCalls()
        {
            /*           give ann error
            System.InvalidOperationException: 'The LINQ expression 'DbSet<CallHistory>()
    .Where(c => (int)c.Status == 0 || (int)c.Status == 1 && DateTime.UtcNow - c.LastHeartbeat > __CALL_TIMEOUT_0)' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'. See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.'
*/
            var staleCalls = await _context.CallHistories
                .Where(c => (c.Status == CallStatus.Initiated || c.Status == CallStatus.Connected) &&
                            DateTime.UtcNow - c.LastHeartbeat > CALL_TIMEOUT)
                .ToListAsync();

            if (!staleCalls.Any())
                return Ok(new { Message = "No stale calls found" });

            foreach (var call in staleCalls)
            {
                call.Status = CallStatus.Disconnected;
                call.EndTime = DateTime.UtcNow;
                call.DisconnectReason = "Call automatically disconnected due to inactivity";

                var post = await _context.Posts.FindAsync(call.PostId);
                if (post != null && post.Status == PostStatus.Accepted)
                {
                    post.Status = PostStatus.Completed;
                    _context.Posts.Update(post);
                }
            }

            _context.CallHistories.UpdateRange(staleCalls);
            await _context.SaveChangesAsync();

            foreach (var call in staleCalls)
            {
                var notificationData = new
                {
                    ChannelName = call.RoomName,
                    Message = "The call was disconnected due to inactivity",
                    Timestamp = DateTime.UtcNow
                };
                await _notificationService.NotifyUserAsync(call.CallerId, notificationData, "CallDisconnected");
                await _notificationService.NotifyUserAsync(call.CalleeId, notificationData, "CallDisconnected");
            }

            return Ok(new
            {
                Message = $"Cleaned up {staleCalls.Count} stale calls",
                CleanedCallIds = staleCalls.Select(c => c.Id).ToList()
            });
        }

        [HttpGet("{userId}/call-history")]
        public async Task<IActionResult> GetUserCallHistory(string userId)
        {
            var callHistory = await _context.CallHistories
                .Where(ch => ch.CallerId == userId || ch.CalleeId == userId)
                .OrderByDescending(ch => ch.StartTime)
                .Select(ch => new CallHistoryDto
                {
                    Id = ch.Id,
                    PostId = ch.PostId,
                    RoomName = ch.RoomName,
                    Caller = new UserDto
                    {
                        Id = ch.Caller.Id,
                        FullName = ch.Caller.FullName,
                        username = ch.Caller.UserName,
                        PictureUrl = ch.Caller.ProfileImageUrl,
                        status = ch.Caller.Status
                    },
                    Callee = new UserDto
                    {
                        Id = ch.Callee.Id,
                        FullName = ch.Callee.FullName,
                        username = ch.Caller.UserName,
                        PictureUrl = ch.Callee.ProfileImageUrl,
                        status = ch.Callee.Status
                    },
                    StartTime = ch.StartTime,
                    EndTime = ch.EndTime,
                    Status = ch.Status.ToString(),
                    Duration = ch.EndTime.HasValue ? (ch.EndTime.Value - ch.StartTime).TotalMinutes : (double?)null
                })
                .ToListAsync();

            return Ok(callHistory);
        }

        [HttpDelete("{callId}")]
        public async Task<IActionResult> DeleteCallHistory(int callId)
        {
            var callHistory = await _context.CallHistories.FindAsync(callId);
            if (callHistory == null)
                return NotFound("Call history not found");

            var currentUserId = _userManager.GetUserId(User);
            if (callHistory.CallerId != currentUserId && callHistory.CalleeId != currentUserId)
                return Forbid("You are not authorized to delete this call record");

            _context.CallHistories.Remove(callHistory);
            await _context.SaveChangesAsync();
            return Ok("Call history deleted successfully");
        }
    }



   

    public class HeartbeatRequest
    {
        public string RoomName { get; set; }
    }

   
}
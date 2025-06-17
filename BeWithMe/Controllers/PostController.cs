using BeWithMe.Data;
using BeWithMe.DTOs;
using BeWithMe.Models.Enums;
using BeWithMe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeWithMe.Services;

namespace BeWithMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Post")]
    public class PostController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public PostController(ApplicationDbContext context,
                                 UserManager<ApplicationUser> userManager,
                                 INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;

        }

        [HttpPost("Create")]
        [Authorize(Roles = RoleConstants.Patient)]
        public async Task<IActionResult> CreatePost([FromBody] PostDTO post)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }


            // Save post to database  
            var newPost = new Post
            {
                UserId = userId,
                Content = post.Content,
                CreatedAt = post.CreatedAt == default ? DateTime.UtcNow : post.CreatedAt,
                //status be default is pending 
            };

            // Retrieve the patient and update NeedsDescription  
            //var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            //if (patient != null)
            //{
            //    patient.NeedsDescription = post.Content;
            //    _context.Patients.Update(patient);
            //}

            await _context.Posts.AddAsync(newPost);
            await _context.SaveChangesAsync();

            // Get patient FullName and Id  
            var patientInfo = await _userManager.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.FullName, u.Id })
                .FirstOrDefaultAsync();

            if (patientInfo == null)
            {
                return BadRequest("Patient information not found");
            }

            // Get all online helpers
            var onlineHelperIds = await _context.hubUserGroups
                .Where(ug => ug.GroupId == _context.hubGroups.FirstOrDefault(g => g.GroupName == "OnlineHelpers").GroupId)
                .Select(ug => ug.UserId)
                .ToListAsync();

            // Create a notification for each helper
            foreach (var helperId in onlineHelperIds)
            {
                await _notificationService.CreateNotificationAsync(
                    userId,         // Creator (patient)
                    helperId,       // Recipient (helper)
                    post.Content,   // Content 
                    NotificationType.Post
                );
            }


            // Create notification data to send to online helpers  
            //var notificationData = new
            //{
            //    Id = notification.Id,
            //    PostId = newPost.Id,
            //    PatientId = patientInfo.Id,
            //    PatientName = patientInfo.FullName,
            //    Content = post.Content.Length > 100 ? post.Content.Substring(0, 97) + "..." : post.Content,
            //    CreatedAt = newPost.CreatedAt
            //};

            // Notify all online helpers  
            await _notificationService.NotifyHelpersAsync("NewPostCreated");

            return Ok(new { postId = newPost.Id, message = "Post created successfully" });

        }

        [HttpGet()]
        [Authorize(Roles = $"{RoleConstants.Helper}")]
        public async Task<IActionResult> Posts()
        {
            //retreiv pending Posts and the bool if the user accept this post
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var pendingPosts = _context.Posts
                .Where(s => s.Status == PostStatus.Pending)
                .Include(p => p.User)
                .Include(p => p.Reactions)
                .Select(p => new retrievePostsDTO
                {
                    Id = p.Id,
                    Content = p.Content,
                    Author = new AuthorDTO
                    {
                        Id = p.UserId,
                        FullName = p.User.FullName,
                        PictureUrl = p.User.ProfileImageUrl,
                        userName = p.User.UserName,
                    },
                    ReactionsCount = p.Reactions.Count,
                    CreatedAt = p.CreatedAt,
                    isAccepted = p.Reactions.Any(r => r.AcceptorId == userId)
                    /*
                     *             bool hasAlreadyAccepted = post.Reactions.Any(r => r.AcceptorId == requestAcceptor.requestAcceptorId);

                    Acceptors = p.Reactions
                    .Select(pr => new AcceptorDTO
                    {
                        Id = pr.AcceptorId,
                        FullName = pr.Helper.User.FullName,
                        Rate = pr.Helper.Rate,
                        PictureUrl = pr.Helper.User.ProfileImageUrl
                    }).ToList()
                    */
                }).ToList();

            if (pendingPosts == null)
            {
                NotFound(new { message = "There is no Pending Posts" });
            }


            return Ok(pendingPosts);
        }


        // Get all posts of the patient
        [HttpGet("MyPosts")]
        [Authorize(Roles = RoleConstants.Patient)]
        public async Task<IActionResult> MyPosts()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }
            var posts = await _context.Posts
                .Where(p => p.UserId == userId)
                .Select(p => new
                {
                    p.Id,
                    p.Content,
                    p.CreatedAt,
                    p.Status
                })
                .ToListAsync();
            return Ok(posts);
        }


        [HttpGet("{postId}/reactions")]
        [Authorize(Roles = $"{RoleConstants.Helper},{RoleConstants.Patient}")]
        public async Task<IActionResult> RetrieveReacitons(int postId)
        {
            var reactions = await _context.PostReactions
                .Include(r => r.Helper)
                .Where(r => r.PostId == postId)
                .Select(r => new
                {
                    r.Helper.User.FullName,
                    r.Helper.User.ProfileImageUrl,
                    r.Helper.Rate,
                    r.AcceptorId,
                    r.PostId,
                    r.Helper.UserId
                })
                .ToListAsync();
            return Ok(reactions);
        }


        [HttpDelete("{postId}")]
        [Authorize(Roles = RoleConstants.Patient)]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized("userid not found");
            }
            // Check if the post exists and belongs to the user
            var post = await _context.Posts.FindAsync(postId);
            if (post == null || post.UserId != userId)
            {
                return NotFound(new { message = "Post not found or you are not authorized to delete it." });
            }
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Post deleted successfully." });
        }

        [HttpPut("{postId}/status")]
        [Authorize(Roles = RoleConstants.Helper)]
        public async Task<IActionResult> UpdatePostStatus(int postId, [FromBody] PostStatus status)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized("userid not found");
            }
            // Check if the post exists and belongs to the user
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound(new { message = "Post not found." });
            }
            post.Status = status;
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Post status updated successfully." });
        }


    }
}

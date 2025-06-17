using BeWithMe.Data;
using BeWithMe.DTOs;
using BeWithMe.Models;
using BeWithMe.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeWithMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Helper")]

    public class HelperController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HelperController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = RoleConstants.Patient)]
        public async Task<IActionResult> Helpers(
           [FromQuery] string search = null,
           [FromQuery] string sort = "asc",
           [FromQuery] bool Status = true,
           [FromQuery] int page = 1,
           [FromQuery] int pageSize = 5)
        {

            // Base query
            var query = _context.Helpers.Include(u => u.User).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(h =>
                    EF.Functions.Like(h.User.FullName, $"%{search}%"));
            }

            // Filter by status
            if (Status)
            {
                query = query.Where(h => h.User.Status == Status);
            }

            // Apply sorting
            query = sort.ToLower() switch
            {
                "desc" =>
                    query.OrderByDescending(h => h.Rate),

                "asc" =>
                    query.OrderBy(h => h.Rate)
            };



            // Pagination
            var totalHelpers = await query.CountAsync();
            var helper = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    Id = p.User.Id,
                    FullName = p.User.FullName,
                    Age = p.User.Age,
                    Rate = p.Rate,
                    ProfileImageUrl = p.User.ProfileImageUrl
                })
                .ToListAsync();


            // Return response
            return Ok(new
            {
                Helpers = helper,
                TotalHelpers = totalHelpers,
                CurrentPage = page,
                PageSize = pageSize
            });
        }


        [HttpGet("{id}")]
        [Authorize(Roles =RoleConstants.Patient)]
        public async Task<IActionResult> GetHelper(string id)
        {
            var helper = await _context.Helpers
                .Include(u => u.User)
                .FirstOrDefaultAsync(h => h.UserId == id);
            if (helper == null)
            {
                return NotFound();
            }
            var response = new
            {
                Id = helper.User.Id,
                FullName = helper.User.FullName,
                Age = helper.User.Age,
                Rate = helper.Rate,
                ProfileImageUrl = helper.User.ProfileImageUrl,
            };
            return Ok(response);
        }

        //Accept Patient Post Request
        [HttpPost("{postId}/Accept-Post")]
        [Authorize(Roles = RoleConstants.Helper)]
        public async Task<IActionResult> AcceptPost(int postId, requestAcceptorDto requestAcceptor)
        {
            // 1. Validate post exists and is pending
            var post = _context.Posts
                .Include(p => p.Reactions)
                .FirstOrDefault(p => p.Id == postId);

            if (post == null) return NotFound("Post not found");
            if (post.Status != PostStatus.Pending) return BadRequest("Only pending posts can be accepted");

            // 2. Validate user exists
            var user = _context.Users.Find(requestAcceptor.requestAcceptorId);
            if (user == null) return NotFound("User not found");

            // 3. Check if user already accepted (optional)
            bool hasAlreadyAccepted = post.Reactions.Any(r => r.AcceptorId == requestAcceptor.requestAcceptorId);
            if (hasAlreadyAccepted) return Ok("User already accepted this post");

            // 5. Update post status to "Accepted"
            //post.Status = PostStatus.Accepted;
            //_context.Posts.Update(post);
            //await _context.SaveChangesAsync();
            // 4. Create reaction
            var reaction = new PostReaction
            {
                PostId = postId,
                AcceptorId = requestAcceptor.requestAcceptorId
            };

            _context.PostReactions.Add(reaction);
            await _context.SaveChangesAsync();

            //return Ok("Reaction saved");

            return Ok("Accepted Send to the Patient Waiting his confirmatio");
        }

        // Remove The Reaction
        [HttpDelete("Remove-Reaction/{postId}")]
        [Authorize(Roles = RoleConstants.Helper)]
        public async Task<IActionResult> RemoveReaction(int postId, requestAcceptorDto requestAcceptor)
        {
            // 1. Validate post exists and is pending
            var post = await _context.Posts
                .Include(p => p.Reactions)
                .FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null) return NotFound("Post not found");
            if (post.Status != PostStatus.Pending) return BadRequest("Only pending posts can be accepted");

            // 2. Validate user exists
            var user = await _context.Users.FindAsync(requestAcceptor.requestAcceptorId);
            if (user == null) return NotFound("User not found");

            // 3. Find the existing reaction to delete
            var reactionToDelete = post.Reactions
                .FirstOrDefault(r =>
                    r.PostId == postId &&
                    r.AcceptorId == requestAcceptor.requestAcceptorId);

            if (reactionToDelete == null)
            {
                return Ok("Reaction not found or already removed");
            }

            // 4. Delete the reaction
            _context.PostReactions.Remove(reactionToDelete);
            await _context.SaveChangesAsync();

            return Ok("Reaction removed successfully");
        }
    }
}

using BeWithMe.Data;
using BeWithMe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BeWithMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[ApiExplorerSettings(GroupName = "v2")]
    [ApiExplorerSettings(GroupName = "Admin")]
    public class AdminController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context
            , ILogger<AdminController> logger, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }


        // admin can delete any user
        [HttpDelete("DeleteUser/{userId}")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Ok(new { message = "User deleted successfully" });
            }
            return BadRequest(new { message = "Error deleting user" });
        }


        // admin can delete any post
        [HttpDelete("DeletePost/{postId}")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound(new { message = "Post not found" });
            }
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Post deleted successfully" });
        }


        #region Seed

        //hide this endpoint from swagger
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("seed-users")]
        public async Task<IActionResult> SeedUsers(string Role, int numUsers)
        {
            Role = Role.ToLower();

            await EnsureRolesExistAsync();
            await SeedUsersWithRoleAsync(Role, numUsers);
            return Ok("Users seeded successfully.");
        }

        private async Task EnsureRolesExistAsync()
        {
            foreach (var role in RoleConstants.AllRoles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private async Task SeedUsersWithRoleAsync(string role, int numUsers)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    for (int i = 1; i <= numUsers; i++)
                    {
                        var email = $"{role}{i}@example.com";
                        var password = "Password123!";

                        if (await _userManager.FindByEmailAsync(email) != null)
                            continue;

                        var user = CreateUser(email, role, i);
                        var result = await _userManager.CreateAsync(user, password);

                        if (result.Succeeded)
                        {
                            await AssignRolesToUserAsync(user, role);
                            await CreateUserProfile(user, role);
                        }
                        else
                        {
                            LogErrors(result.Errors, email);
                        }
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error during user seeding");
                    throw;
                }
            });
        }
        private static readonly Random _random = new Random();
        private static readonly string[] Genders = { "Male", "Female" };
        private ApplicationUser CreateUser(string email, string role, int i)
        {
            var imgdefaultPath = Path.Combine("uploads", "imgs", "defaultimg.jpg");

            return new ApplicationUser
            {

                UserName = $"{role}{i}",
                FullName = $"{role}{i}",
                Email = email,
                CreatedAt = DateTime.UtcNow,
                ProfileImageUrl = imgdefaultPath,
                Gender = Genders[_random.Next(Genders.Length)] // Randomly selects from the array


            };
        }

        private async Task AssignRolesToUserAsync(ApplicationUser user, string role)
        {
            var rolesToAssign = new[] { role };
            await _userManager.AddToRolesAsync(user, rolesToAssign);
        }
        private async Task CreateUserProfile(ApplicationUser user, string role)
        {
            switch (role.ToLower())
            {
                case "patient":
                    await CreatePatientProfile(user);
                    break;

                case "helper":
                    await CreateHelperProfile(user);
                    break;
            }
        }

        private async Task CreatePatientProfile(ApplicationUser user)
        {
            var patient = new Patient
            {
                UserId = user.Id,
                NeedsDescription = "",
                HelpCount = 0
            };

            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();
        }

        private async Task CreateHelperProfile(ApplicationUser user)
        {
            var helper = new Helper
            {
                UserId = user.Id,
                Rate = 0m
            };

            await _context.Helpers.AddAsync(helper);
            await _context.SaveChangesAsync();

        }


        private void LogErrors(IEnumerable<IdentityError> errors, string email)
        {
            foreach (var error in errors)
            {
                _logger.LogError($"Error creating user {email}: {error.Description}");
            }
        } 
        #endregion
    }
}



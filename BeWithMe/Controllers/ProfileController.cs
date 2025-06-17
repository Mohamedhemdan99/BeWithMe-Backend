using BeWithMe.Data;
using BeWithMe.DTOs;
using BeWithMe.Models;
using BeWithMe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeWithMe.Controllers
{



    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Ensure only authenticated users can access this endpoint
    [ApiExplorerSettings(GroupName = "Common")]
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;


        public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        /// <summary>
        /// Gets the profile information for the currently authenticated user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Profile()
        {

            // Fetch the user (EF will return Patient/Helper subclass based on Discriminator)
            var userId = _userManager.GetUserId(User);

            var user = await _context.Users
                .Include(u => u.Patient)
                .Include(u => u.Helper)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            // Check the user's type
            if (user.Patient != null)
            {
                return Ok(new PatientProfileDto
                {   userId = user.Id,
                    FullName = user.FullName,
                    userName = user.UserName,
                    Gender = user.Gender,
                    DateOfBirth = user.DateOfBirth,
                    Email = user.Email,
                    LanguagePreference = user.LanguagePreference,
                    //NeedsDescription = user.Patient.NeedsDescription,
                    HelpCount = user.Patient.HelpCount,
                    ProfileImageUrl = user.ProfileImageUrl
                });
            }
            else if (user.Helper != null)
            {
                return Ok(new HelperProfileDto
                {   userId = user.Id,
                    FullName = user.FullName,
                    userName = user.UserName,
                    Gender = user.Gender,
                    DateOfBirth = user.DateOfBirth,
                    Email = user.Email,
                    LanguagePreference = user.LanguagePreference,
                    Rate = user.Helper.Rate,
                    ProfileImageUrl = user.ProfileImageUrl,
                });
            }

            // Fallback for base ApplicationUser (not recommended)
            return BadRequest("User has no associated profile type");

        }

        [HttpPut]
        public async Task<IActionResult> Profile( [FromForm] ProfileDTO dto)
        {
            try
            {


                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }
                var user = await _context.Users
                    .Include(u => u.Patient)
                    .Include(u => u.Helper)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null) return NotFound("User Not Found");

                if (dto.FullName != null && dto.FullName != "string") user.FullName = dto.FullName;
                if (dto.Gender != null && dto.Gender != "string") user.Gender = dto.Gender;
                if (dto.DateOfBirth.HasValue) user.DateOfBirth = dto.DateOfBirth.Value;
                if (dto.LanguagePreference != null && dto.LanguagePreference != "string") user.LanguagePreference = dto.LanguagePreference;
                if (dto.Password != null && dto.Password != "string" )
                {
                    if(dto.Password != dto.ConfirmPassword)
                        return BadRequest("Passwords do not match.");
                    if (dto.Password.Length < 6)
                        return BadRequest("Password must be at least 6 characters long.");

                    var passwordHasher = new PasswordHasher<ApplicationUser>();
                    user.PasswordHash = passwordHasher.HashPassword(user, dto.Password);
                }
                if (dto.ProfileImage != null && dto.ProfileImage.Length > 0)
                {
                    var isimage = new CheckImage();
                    if (!isimage.IsImage(dto.ProfileImage))
                        return BadRequest("Invalid file type. Only images allowed.");

                    if (dto.ProfileImage.Length > 5 * 1024 * 1024) // Max 5MB
                        return BadRequest("File too large (max 5MB).");

                    var fileName = user.Id + Guid.NewGuid() + Path.GetExtension(dto.ProfileImage.FileName);
                    // generate file path to wrok on any server dynamicly
                    if (fileName != null) {
                        fileName = fileName.Replace(" ", "_");
                    }
                    //var uploadsFolder = Path.Combine(_env.ContentRootPath, "uploads", "imgs",fileName);


                    var uploadsFolder = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", "imgs",fileName);

                    //var filePath = Path.Combine(Environme"uploads", "imgs", fileName);
                    using (var stream = new FileStream(uploadsFolder, FileMode.Create))
                    {
                        await dto.ProfileImage.CopyToAsync(stream);
                    }
                    user.ProfileImageUrl = $"uploads/imgs/{fileName}";
                }


                //if (user.Patient != null) // User is a patient
                //{
                //    if (dto.NeedsDescription != null && dto.NeedsDescription != "string") user.Patient.NeedsDescription = dto.NeedsDescription;
                //}

                await _context.SaveChangesAsync();



                return Ok(new { Message = "Profile updated successfully.", imageUrl = user.ProfileImageUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        

        
    }
}






using BeWithMe.Data;
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
    [Authorize]
    [ApiExplorerSettings(GroupName = "Patient")]

    public class PatientController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public PatientController(ApplicationDbContext context,
                                 UserManager<ApplicationUser> userManager,
                                 INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;

        }

        /// <summary>
        /// Retrieve a paginated list of patients with filters and sorting.
        /// </summary>
        /// <param name="search">Search term for FullName or NeedsDescription.</param>
        /// <param name="sort">Sorting criteria (helpCountAsc, helpCountDesc).</param>
        /// <param name="page">Page number for pagination (default: 1).</param>
        /// <param name="pageSize">Number of items per page (default: 10).</param>
        /// <returns>Paginated list of patients with metadata.</returns>
        [HttpGet]
        //[Authorize(Roles = RoleConstants.Helper)]
        public async Task<IActionResult> Patients(
            [FromQuery] string search = null,
           [FromQuery] bool Status = true,
           [FromQuery] string sort = "helpCountDesc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5)
        {

            // Base query
            var query = _context.Patients.Include(u => u.User).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    EF.Functions.Like(p.User.FullName, $"%{search}%") ||
                    EF.Functions.Like(p.NeedsDescription ?? "", $"%{search}%"));
            }

            // Apply status filter
            if (Status)
            {
                query = query.Where(p => p.User.Status == Status);
            }

            // Apply sorting
            switch (sort.ToLower())
            {
                case "helpcountasc":
                    query = query.OrderBy(p => p.HelpCount);
                    break;
                case "helpcountdesc":
                    query = query.OrderByDescending(p => p.HelpCount);
                    break;
                default:
                    query = query.OrderByDescending(p => p.HelpCount); // Default sorting
                    break;
            }

            // Pagination
            var totalPatients = await query.CountAsync();
            var patients = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    Id = p.User.Id,
                    FullName = p.User.FullName,
                    Age = p.User.Age,
                    HelpCount = p.HelpCount,
                    NeedsDescription = p.NeedsDescription,
                    ProfileImageUrl = p.User.ProfileImageUrl
                })
                .ToListAsync();


            // Return response
            return Ok(new
            {
                Patients = patients,
                TotalPatients = totalPatients,
                CurrentPage = page,
                PageSize = pageSize
            });
        }


        // Retreive a specific patient by ID
        [HttpGet("{id}")]
        //[Authorize(Roles = RoleConstants.Helper)]
        public async Task<IActionResult> GetPatient(string id)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User.Id == id);
            if (patient == null)
            {
                return NotFound("Patient not found");
            }
            var patientInfo = new
            {
                Id = patient.User.Id,
                FullName = patient.User.FullName,
                Age = patient.User.Age,
                HelpCount = patient.HelpCount,
                //NeedsDescription = patient.NeedsDescription,
                ProfileImageUrl = patient.User.ProfileImageUrl
            };
            return Ok(patientInfo);
        }






    }
}



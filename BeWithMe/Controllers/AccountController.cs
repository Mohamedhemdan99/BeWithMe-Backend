using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using BeWithMe.Data;
using BeWithMe.DTOs;
using BeWithMe.Models;
using BeWithMe.Repository.Interfaces;
using BeWithMe.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BeWithMe.Controllers
{
    #region
    //[Route("api/[controller]")]
    //[ApiController]
    //public class AccountController : ControllerBase
    //{
    //    private readonly UserManager<ApplicationUser> _userManager;
    //    private readonly IRepository<ApplicationUser> repository;
    //    private readonly IConfiguration _configuration;

    //    public AccountController(UserManager<ApplicationUser> userManager,
    //                                IRepository<ApplicationUser> repository, IConfiguration configuration)
    //    {
    //        this._userManager = userManager;
    //        this.repository = repository;
    //        this._configuration = configuration;
    //    }

    //    [HttpPost("Register")]
    //    [ProducesResponseType(StatusCodes.Status201Created)]
    //    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    //    public async Task<IActionResult> RegisterAsync(RegisterUserDTO userDTO)
    //    {
    //        if (!ModelState.IsValid)
    //            return BadRequest(ModelState);

    //        var userExist = await _userManager.FindByEmailAsync(userDTO.Email);
    //        if (userExist != null)
    //        {
    //            return BadRequest("User Exist");
    //        }

    //        ApplicationUser newUser = new()
    //        {
    //            Email = userDTO.Email,
    //            SecurityStamp = Guid.NewGuid().ToString(),
    //            UserName = userDTO.Username
    //        };

    //        var result = await _userManager.CreateAsync(newUser, userDTO.Password);
    //        if (!result.Succeeded)
    //        {
    //            return BadRequest(result.Errors.FirstOrDefault()?.Description);
    //        }

    //        // Assign both roles to the user
    //        await _userManager.AddToRoleAsync(newUser, "User");
    //        await _userManager.AddToRoleAsync(newUser, "Helper");

    //        // Fetch the roles assigned to the user
    //        var roles = await _userManager.GetRolesAsync(newUser);

    //        // Generate JWT token
    //        var token = GenerateJwtToken(newUser, roles);
    //        return Ok(new { Token = token, Message = "User Created Successfully", Roles = roles });
    //    }

    //    private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
    //    {
    //        var tokenHandler = new JwtSecurityTokenHandler();
    //        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

    //        var claims = new List<Claim>
    //        {
    //            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
    //            new Claim(JwtRegisteredClaimNames.Email, user.Email),
    //            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    //        };

    //        foreach (var role in roles)
    //        {
    //            claims.Add(new Claim(ClaimTypes.Role, role));
    //        }

    //        var tokenDescriptor = new SecurityTokenDescriptor
    //        {
    //            Subject = new ClaimsIdentity(claims),
    //            Expires = DateTime.UtcNow.AddHours(
    //                _configuration.GetValue<double>("Jwt:ExpirationMinutes")),
    //            Issuer = _configuration["Jwt:Issuer"],
    //            Audience = _configuration["Jwt:Audience"],

    //            SigningCredentials = new SigningCredentials(
    //                new SymmetricSecurityKey(key),
    //                SecurityAlgorithms.HmacSha256Signature)
    //        };

    //        var token = tokenHandler.CreateToken(tokenDescriptor);
    //        return tokenHandler.WriteToken(token);
    //    }
    //}
    #endregion




    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("registration")] // Apply rate limiting policy
    [ApiExplorerSettings(GroupName = "Common")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            IRepository<ApplicationUser> repository,
            IConfiguration configuration,
            ILogger<AccountController> logger,
            IEmailService emailService,
            ApplicationDbContext context,
            IWebHostEnvironment env,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _env = env;
            _signInManager = signInManager;
        }

        [HttpPost("Register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> RegisterAsync([FromForm] RegisterUserDTO userDTO)
        {

            if (userDTO.Role.ToLower() != RoleConstants.Patient && userDTO.Role.ToLower() != RoleConstants.Helper)
            {
                return BadRequest(new { Error = "Invalid role. Must be 'Patient' or 'Helper'." });
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("User registration failed due to invalid model state");
                return BadRequest(new { Error = "Invalid registration data", Details = ModelState });
            }

            if (!IsStrongPassword(userDTO.Password))
            {
                return BadRequest(new { Error = "Password too weak" });
            }

            // Check username format
            if (string.IsNullOrWhiteSpace(userDTO.Username) || userDTO.Username.Length < 3)
            {
                return BadRequest(new { Error = "Username must be at least 3 characters" });
            }

            //Check if username exists
            var userNameExists = await _userManager.FindByNameAsync(userDTO.Username);
            if (userNameExists != null)
            {
                return BadRequest(new { Error = "The user name is already teken" });
            }

            var normalizedEmail = userDTO.Email.ToLowerInvariant();

            var result = await ExecuteRegistrationAsync(normalizedEmail, userDTO);
            return result;
        }

        private async Task<IActionResult> ExecuteRegistrationAsync(string normalizedEmail, RegisterUserDTO userDTO)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    string filePath = "";
                   string fileName = "";
                    
                    var userExist = await _userManager.FindByEmailAsync(normalizedEmail);
                    if (userExist != null)
                    {
                        return BadRequest(new { Error = "Email already registered" });
                    }
                    var imagePath = "";
                    //check if there is an image then it exist generate a new name for it and save it if it doesn't uploaded put default url for it 
                    if (userDTO.ProfileImage != null && userDTO.ProfileImage.Length > 0)
                    {
                        var isimage = new CheckImage();
                        if (!isimage.IsImage(userDTO.ProfileImage))
                            return BadRequest(new { Error = "Invalid file type. Only images allowed." });
                        if (userDTO.ProfileImage.Length > 5 * 1024 * 1024) // Max 5MB
                            return BadRequest(new { Error = "File too large (max 5MB)." });
                        fileName = userDTO.Username + Guid.NewGuid() + Path.GetExtension(userDTO.ProfileImage.FileName);
                        fileName = fileName.Replace(" ", "_");
                        filePath = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", "imgs", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await userDTO.ProfileImage.CopyToAsync(stream);
                        }
                         imagePath = Path.Combine("uploads", "imgs", fileName);
                    }
                    else
                    {
                        fileName = "default.png";
                        filePath = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", "imgs", fileName);
                         imagePath = Path.Combine("uploads", "imgs", fileName);
                    }

                    var newUser = new ApplicationUser
                    {
                        Email = userDTO.Email,
                        UserName = userDTO.Username,
                        CreatedAt = DateTime.UtcNow,
                        FullName = userDTO.Username,
                        Gender = userDTO.Gender,                        
                        DateOfBirth = userDTO.DateOfBirth,
                        ProfileImageUrl = imagePath,
                        LanguagePreference = "Arabic"
                    };

                    var createResult = await _userManager.CreateAsync(newUser, userDTO.Password);
                    if (!createResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { Error = "Registration failed", Details = createResult.Errors.Select(e => e.Description) });
                    }


                    switch (userDTO.Role.ToLower())
                    {
                        case RoleConstants.Patient:
                            await CreatePatientProfile(newUser);
                            break;

                        case RoleConstants.Helper:
                            await CreateHelperProfile(newUser);
                            break;
                    }
                    await _userManager.AddToRoleAsync(newUser, userDTO.Role);

                    await transaction.CommitAsync();

                    var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                    var roles = await _userManager.GetRolesAsync(newUser);
                    var token = GenerateJwtToken(newUser, roles);

                    return StatusCode(StatusCodes.Status201Created, new
                    {
                        Message = "User created successfully. Please check your email to confirm your account.",
                        UserId = newUser.Id,
                        Role = roles.FirstOrDefault(r => r == RoleConstants.Helper || r == RoleConstants.Patient),
                        Token = token
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Registration error");
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Registration failed due to an internal error" });
                }
            });
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


        [HttpPost("Login")]
        [EnableRateLimiting("standard")] // Apply rate limiting
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginUserDTO model)
        {

            ApplicationUser user = null;
            if (IsValidEmail(model.UsernameOrEmail))
            {
                var normalizedEmail = _userManager.NormalizeEmail(model.UsernameOrEmail);
                user = await _userManager.FindByEmailAsync(normalizedEmail);
            }
            else
            {
                user = await _userManager.FindByNameAsync(model.UsernameOrEmail);
            }

            if (user == null)
            {
                return NotFound("Invalid login attempt.");
            }

            // Check the Passwrod 
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
          var sigin= await _signInManager.PasswordSignInAsync(user, model.Password ,model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                //await _userManager.ResetAccessFailedCountAsync(user);
                // Get user roles
                    var roles = await _userManager.GetRolesAsync(user);
                // Generate and return JWT token
                var token = GenerateJwtToken(user, roles);
                // Update last login timestamp
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                return Ok(new { message = "Login successful.", userId=user.Id, Token = token, Role = roles.FirstOrDefault(r => r == RoleConstants.Patient
                || r == RoleConstants.Helper) });
            }
            else if (result.IsLockedOut)
            {
                return Unauthorized("User account locked out.");
            }
            else
            {

                return Unauthorized("Email or Password is Wrong.");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }



        [HttpPost("signout")]
        public async Task<IActionResult> LogoutAsync()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return Ok(new { Message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user logout");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = "Logout failed due to an internal error" });
            }
        }

        // Helper method to generate JWT token
        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
            {
                _logger.LogCritical("JWT key is missing or too short");
                throw new InvalidOperationException("Invalid JWT configuration");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtKey);

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        };

            // Add user roles as claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Get token expiration time from configuration with fallback
            //double expirationMinutes = 30; // Default fallback
            //if (!double.TryParse(_configuration["Jwt:ExpirationMinutes"], out expirationMinutes))
            //{
            //    _logger.LogWarning("Invalid JWT expiration configuration, using default");
            //}

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                //Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
                //NotBefore = DateTime.UtcNow // Token is not valid before current time
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }



        // Helper method to validate password strength
        private bool IsStrongPassword(string password)
        {
            // Implement strong password validation logic
            return !string.IsNullOrWhiteSpace(password) &&
                   password.Length >= 8 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit) &&
                   password.Any(c => !char.IsLetterOrDigit(c));
        }
        private string GenerateSixDigitCode()
        {
            // Use the cryptographically secure random number generator for better security
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                // Create a byte array to hold the random values
                byte[] randomNumber = new byte[4];

                // Fill the array with random values
                rng.GetBytes(randomNumber);

                // Convert the random bytes to an integer and take modulo to get a 6-digit number
                int value = BitConverter.ToInt32(randomNumber, 0) & 0x7FFFFFFF; // Ensure positive number
                value = value % 900000 + 100000; // Ensures a 6-digit number (between 100000 and 999999)

                return value.ToString();
            }
        }


        [HttpPost("SendCode")]
        public async Task<IActionResult> SendVerificationCode(SendCodeReuestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userfromdb = await _userManager.FindByEmailAsync(request.Email);
            if (userfromdb == null)
            {
                _logger.LogWarning($"Password reset attempted for non-existent email: {request.Email}");
                //_logger.LogError("Invalid Email Or Doesn't Exist");
                return Ok(new { message = "If your email is registered, you will receive a password reset code." });

            }

            try
            {
                var code = GenerateSixDigitCode();
                var token = await _userManager.GeneratePasswordResetTokenAsync(userfromdb);

                var passwordResetToken = new PasswordResetToken
                {
                    UserId = userfromdb.Id,
                    Code = code,
                    Token = token,
                    Email = userfromdb.Email,
                    ExpirationTime = DateTime.UtcNow.AddMinutes(5)
                };

                var existingTokens = _context.PasswordResetTokens
                                                    .Where(t => t.UserId == userfromdb.Id)
                                                    .ToList();

                if (existingTokens.Any())
                {
                    _context.PasswordResetTokens.RemoveRange(existingTokens);
                }

                _context.PasswordResetTokens.Add(passwordResetToken);
                await _context.SaveChangesAsync();

                var resetUrl = $"{_configuration["URL"]}api/Account/VerifyToken?token={WebUtility.UrlEncode(token)}&email={WebUtility.UrlEncode(userfromdb.Email)}";
                //var resetUrl = $"{_configuration["URL"]}/Account/VerifyToken?Token={token}&Email={userfromdb.Email}";
                //var resetUrl = $"{_configuration["URL"]}/Account/VerifyToken?Token={Uri.EscapeDataString(token)}&Email={Uri.EscapeDataString(userfromdb.Email)}";

                var body = $@"      <p>Hello {userfromdb.UserName},</p>
                                    <p>We received a request to reset your password. Please use the following code below:</p>
                                    <p>Code: <b>{code}</b></p>
                                    <p>If you did not request this change, please ignore this email.</p>
                                    <p>This code will expire in 15 minutes.</p>";

                var Message = new Message(new string[] { userfromdb.Email }, "Reset Your Password", body);

                _emailService.SendEmail(Message);
                _logger.LogInformation($"Password reset code sent to {userfromdb.Email}");
                return Ok(new { message = "If your email is registered, you will receive a password reset code." });
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send verification code to {request.Email}");
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }



        }

        [HttpPost("VerifyCode")]
        public async Task<IActionResult> VerifyCodeAsync(VerifyCodeRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid");
            }
            var userfromdb = await _userManager.FindByEmailAsync(request.Email);
            if (userfromdb == null)
            {
                _logger.LogWarning($"Verification attempted for non-existent email: {request.Email}");
                return BadRequest(new { message = "Invalid verification code." });
            }
            var passwordReset = _context.PasswordResetTokens.FirstOrDefault(u => u.UserId == userfromdb.Id && u.Code == request.Code);
            if (passwordReset == null)
            {
                _logger.LogWarning($"Invalid verification code attempt for user: {userfromdb.Id}");
                return BadRequest(new { message = "Invalid verification code." });
            }

            if (passwordReset.ExpirationTime < DateTime.UtcNow)
            {
                _logger.LogWarning($"Expired verification code attempt for user: {userfromdb.Id}");
                return BadRequest(new { message = "Verification code has expired. Please request a new code." });
            }


            return Ok(new { ResetToken = passwordReset.Token });
        }

        #region VerifyToken

        //[HttpGet("VerifyToken")]
        //[AllowAnonymous]
        //[Consumes("application/json")]
        //private async Task<IActionResult> VerifyTokenAsync([FromQuery] TokenVerificationDTO request)
        //{
        //    if (!ModelState.IsValid) { return BadRequest(new { message = "Email and token are required." }); }
        //    var userfromdb = await _userManager.FindByEmailAsync(request.Email);
        //    if (userfromdb == null)
        //    {
        //        _logger.LogWarning($"Token verification attempted for non-existent email: {request.Email}");
        //        return BadRequest(new { message = "Invalid or expired token." });
        //    }

        //    //var isTokenValid = _applicationDbContext.PasswordResetTokens.FirstOrDefault(u => u.UserId == userfromdb.Id && u.Token == request.Token);

        //    var passwordReset = _context.PasswordResetTokens
        //            .FirstOrDefault(u => u.UserId == userfromdb.Id && u.Token == request.Token);

        //    if (passwordReset == null)
        //    {
        //        _logger.LogWarning($"Invalid token verification attempt for user: {userfromdb.Id}");
        //        return BadRequest(new { message = "Invalid or expired token." });
        //    }

        //    if (passwordReset.ExpirationTime < DateTime.UtcNow)
        //    {
        //        _logger.LogWarning($"Expired token verification attempt for user: {userfromdb.Id}");
        //        return BadRequest(new { message = "Invalid or expired token." });
        //    }

        //    return Ok(new { message = $@"
        //                            <p>Hello {userfromdb.UserName},</p>
        //                            <p>We received a request to reset your password. Please use the following code or click the link below:</p>
        //                            <p>Code: <b></b></p>
        //                            <p><a href="""">Reset Password</a></p>
        //                            <p>If you did not request this change, please ignore this email.</p>
        //                            <p>This code will expire in 15 minutes.</p>"
        //});
        //} 
        #endregion


        [HttpPost("ResetPassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResestPasswordAsync(ResetPasswordDTO request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid ModelState");
                return BadRequest("Invalid");
            }
            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest(new { message = "Password and confirmation password do not match." });
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning($"Password reset attempted for non-existent email: {request.Email}");
                return BadRequest(new { message = "Invalid reset request." });
            }

            var passwordReset = await _context.PasswordResetTokens
                    .FirstOrDefaultAsync(u => u.UserId == user.Id && u.Token == request.Token);

            if (passwordReset == null || passwordReset.ExpirationTime < DateTime.UtcNow)
            {
                _logger.LogWarning($"Invalid or expired token used for password reset: {user.Id}");
                return BadRequest(new { message = "Invalid or expired token." });
            }


            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);
            if (result.Succeeded)
            {
                // Remove the used token from database
                _context.PasswordResetTokens.Remove(passwordReset);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password reset successful for user: {user.Id}");
                return Ok(new { message = "Password reset successfully." });
            }

            _logger.LogWarning($"Password reset failed for user: {user.Id}, Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }


        // delete account 
        [HttpDelete("DeleteAccount")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAccountAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "User ID is required." });
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }
            // check if the user is admin
            var isAdmin = await _userManager.IsInRoleAsync(user, RoleConstants.Admin);
            if (isAdmin)
            {
                return BadRequest(new { message = "Admin accounts cannot be deleted. ارجع للمعلم محمد حمدان" });
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Errors != null && result.Errors.Any())
            {
                _logger.LogWarning($"Account deletion failed for user: {user.Id}, Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }
            if (result.Succeeded)
            {
                return Ok(new { message = "Account deleted successfully." });
            }
            return BadRequest(new { Errors = result?.Errors?.Select(e => e.Description) });
        }

    }
}

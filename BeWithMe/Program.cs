using BeWithMe.Data;
using BeWithMe.Models;
using BeWithMe.Repository.Interfaces;
using BeWithMe.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using BeWithMe.DTOs;
using BeWithMe.Services;
using Microsoft.Extensions.FileProviders;
using System.Text.Json;
using BeWithMe.Hubs;
using Microsoft.Extensions.Options;
using Twilio.Base;

namespace BeWithMe
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            ConfigureServices(builder);

            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    await DbSeeder.SeedDataAsync(services);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            // Configure the HTTP request pipeline
            ConfigureMiddleware(app);


            app.Run();
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            // Add controllers
            builder.Services.AddControllers();
            builder.Services.AddHttpClient(); // Required for HttpClientFactory
            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
           
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions => sqlOptions.EnableRetryOnFailure()),
            ServiceLifetime.Scoped);  // Scoped is the default and correct lifetime

            //Configure Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredUniqueChars = 0;

                // Additional recommended settings
                options.User.RequireUniqueEmail = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromMinutes(15);
            });

            //Configure Authentication
            ConfigureAuthentication(builder);

            //Configure CORS
            builder.Services.AddCors(options =>
            {
                // cannot use WithOrigins("*") with AllowAnyOrigin
                // because it will not work with credentials
                // Cann't use AllowAnyOrigin with AllowCredentials

                options.AddPolicy("AllowedOrigins", policy =>
                {
                    policy
               //.AllowAnyOrigin()
               //.WithOrigins("*") // Replace with your frontend URL
                //.SetIsOriginAllowedToAllowWildcardSubdomains()
               //.WithOrigins("http://localhost:9999") // Replace with your frontend URL
                .SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
                });


                options.AddPolicy("AllowAllOrigins", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                           //.AllowCredentials();
                });
                // allow github pages
                options.AddPolicy("AllowGitHubPages", builder =>
                {
                    builder.WithOrigins("https://mohamedhemdan99.github.io")
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });

            });

            // Configure Swagger
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("Common", new OpenApiInfo
                {
                    Title = "BeWithMe API",
                    Version = "v1",
                    Description = "API documentation for BeWithMe application"
                });

                c.SwaggerDoc("Admin", new OpenApiInfo
                {
                    Title = "Admin Castle",
                    Version = "v1.0",
                    Description = "This own to Eng Mohamed Hemdan wa 7ashiatoh",
                });

                c.SwaggerDoc("Patient", new OpenApiInfo
                {
                    Title = "Patient",
                    Version = "v1.0",
                    Description = "Show the Patient Function",
                });
                c.SwaggerDoc("Helper", new OpenApiInfo
                {
                    Title = "Helper",
                    Version = "v1.0",
                    Description = "Show the Helper Function",
                });
                c.SwaggerDoc("Post", new OpenApiInfo
                {
                    Title = "Post",
                    Version = "v1.0",
                    Description = "Show the Post Function",
                });
                c.SwaggerDoc("Calls", new OpenApiInfo
                {
                    Title = "Calls",
                    Version = "v1.0",
                    Description = "Show the Post Function",
                });

                // Configure Swagger to use JWT Authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services.Configure<EmailConfiguration>(
            builder.Configuration.GetSection("EmailConfiguration")
            );

            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true; // Useful for debugging
                
            });

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(1190, listenOptions =>
                {
                    listenOptions.UseHttps(); // Ensure HTTPS is used for WebSockets (wss://)
                });
            });

            


            // Configure Repository pattern
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<EmailConfiguration>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
        }



        private static void ConfigureAuthentication(WebApplicationBuilder builder)
        {
            var jwtKey = builder.Configuration["Jwt:Key"] ??
                throw new InvalidOperationException("JWT:Key is not configured in appsettings.json");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                //options.RequireHttpsMetadata = builder.Environment.IsProduction();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrEmpty(builder.Configuration["Jwt:Issuer"]),
                    ValidateAudience = !string.IsNullOrEmpty(builder.Configuration["Jwt:Audience"]),
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        // Check if the request is for the SignalR hub and has an access token
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/notificationHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        }

        private static void ConfigureMiddleware(WebApplication app)
        {


            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = ""; // Serve Swagger at the root URL
                options.SwaggerEndpoint("/swagger/Common/swagger.json", "Common");
                options.SwaggerEndpoint("/swagger/Patient/swagger.json", "Patient");
                options.SwaggerEndpoint("/swagger/Helper/swagger.json", "Helper");
                options.SwaggerEndpoint("/swagger/Post/swagger.json", "Post");
                options.SwaggerEndpoint("/swagger/Calls/swagger.json", "Calls");
                options.SwaggerEndpoint("/swagger/Admin/swagger.json", "Admin");
            });

            if (app.Environment.IsProduction())
            {
                // Production middleware
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // Common middleware
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseStaticFiles();
            // Enable CORS
            app.UseCors("AllowedOrigins");
            app.UseCors("AllowAllOrigins");
            app.UseCors("AllowGitHubPages");
            //app.UseWebSockets();
            // Security middleware
            app.UseAuthentication(); // This was commented out in original code
            app.UseAuthorization();
            // Endpoint mapping
            app.MapControllers();
            app.MapHub<NotificationHub>("/notificationHub");
            app.MapHub<CallHub>("/callHub");


        }
    }
}

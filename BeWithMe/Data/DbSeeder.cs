using BeWithMe.Models;
using BeWithMe.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeWithMe.Data
{
    public static class DbSeeder
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                // Ensure database is created
                await context.Database.MigrateAsync();

                // Check if we already have users
                if (userManager.Users.Any())
                {
                    return; // Database has been seeded already
                }

                // Seed roles
                await SeedRolesAsync(roleManager);

                // Seed users
                var patientUsers = await SeedPatientsAsync(userManager);
                var helperUsers = await SeedHelpersAsync(userManager);
                var adminUsers = await SeedAdminsAsync(userManager);

                // Seed posts
                await SeedPostsAsync(context, patientUsers);

                // Seed post reactions
                await SeedPostReactionsAsync(context, helperUsers);

                // Seed notifications
                await SeedNotificationsAsync(context);

                // Seed call history
                await SeedCallHistoryAsync(context);
            }
            catch (Exception ex)
            {
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("DataSeeding");
                //var logger = services.GetRequiredService<ILogger<DbSeeder>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = {RoleConstants.Admin, RoleConstants.Patient, RoleConstants.Helper };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task<List<ApplicationUser>> SeedPatientsAsync(UserManager<ApplicationUser> userManager)
        {
            // Create sample patient users
            var patientUsers = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    UserName = "patient1",
                    Email = "patient1@example.com",
                    FullName = "Ahmed Mohammed",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1990, 5, 15),
                    ProfileImageUrl = "uploads/imgs/default.png",
                    PhoneNumber = "9665XXXXXXX1",
                    EmailConfirmed = true,
                    Patient = new Patient()


                },
                new ApplicationUser
                {
                    UserName = "patient2",
                    Email = "patient2@example.com",
                    FullName = "Fatima Ali",
                    Gender = "Female",
                    DateOfBirth = new DateTime(1995, 8, 20),
                    ProfileImageUrl = "uploads/imgs/default.png",
                    PhoneNumber = "9665XXXXXXX2",
                    EmailConfirmed = true,
                    Patient = new Patient()


                },
                new ApplicationUser
                {
                    UserName = "patient3",
                    Email = "patient3@example.com",
                    FullName = "Khalid Abdullah",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1988, 3, 10),
                    ProfileImageUrl = "uploads/imgs/default.png",
                    PhoneNumber = "9665XXXXXXX3",
                    EmailConfirmed = true,
                    Patient = new Patient()


                },
                new ApplicationUser
                {
                    UserName = "patient4",
                    Email = "patient4@example.com",
                    FullName = "Mohamed Abdullah",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1988, 3, 10),
                    ProfileImageUrl = "uploads/imgs/default.png",
                    PhoneNumber = "9665XXXXXXX3",
                    EmailConfirmed = true,
                    Patient = new Patient()


                },
                new ApplicationUser
                {
                    UserName = "patient5",
                    Email = "patient5@example.com",
                    FullName = "Hossam Hazme",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1988, 3, 10),
                    ProfileImageUrl = "uploads/imgs/default.png",
                    PhoneNumber = "9665XXXXXXX3",
                    EmailConfirmed = true,
                    Patient = new Patient()


                }
            };

            foreach (var user in patientUsers)
            {
                var result = await userManager.CreateAsync(user, "Patient123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, RoleConstants.Patient);
                }
            }

            return patientUsers;
        }

        private static async Task<List<ApplicationUser>> SeedHelpersAsync(UserManager<ApplicationUser> userManager)
        {
            // Create sample helper users
            var helperUsers = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    UserName = "helper1",
                    Email = "helper1@example.com",
                    FullName = "Dr. Sara Mahmoud",
                    Gender = "Female",
                    DateOfBirth = new DateTime(1980, 2, 25),
                    ProfileImageUrl ="uploads/imgs/default.png",
                    PhoneNumber = "9665XXXXXXX4",
                    EmailConfirmed = true,
                    Helper = new Helper
                    {
                        Rate = 4.8m
                    }
                },
                new ApplicationUser
                {
                    UserName = "helper2",
                    Email = "helper2@example.com",
                    FullName = "Dr. Mohamed Aly",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1975, 11, 15),
                    ProfileImageUrl = "uploads/imgs/default.png",
                    PhoneNumber = "9665XXXXXXX5",
                    EmailConfirmed = true,
                    Helper = new Helper
                    {
                        Rate = 4.5m
                    }
                }
            };

            foreach (var user in helperUsers)
            {
                var result = await userManager.CreateAsync(user, "Helper123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, RoleConstants.Helper);
                }
            }

            return helperUsers;
        }

        private static async Task<List<ApplicationUser>> SeedAdminsAsync(UserManager<ApplicationUser> userManager)
        {
            var helperUsers = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    UserName = "mohamedhemdan",
                    Email = "mohamedhemdanismail2@gmail.com",
                    FullName = "Dr. Mohamed Hemdan",
                    Gender = "Male",
                    DateOfBirth = new DateTime(2003, 5, 5),
                    ProfileImageUrl ="uploads/imgs/default.png",
                    PhoneNumber = "9665XXXXXXX4",
                    EmailConfirmed = true,
                    Helper = new Helper
                    {
                        Rate = 5m
                    }
                },
                new ApplicationUser
                {
                    UserName = "hossamhazem",
                    Email = "hossamhazem@example.com",
                    FullName = "Dr. Hossam Hazem",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1975, 11, 15),
                    ProfileImageUrl = "uploads/imgs/default.png",
                    PhoneNumber = "9665XXXXXXX5",
                    EmailConfirmed = true,
                    Helper = new Helper
                    {
                        Rate = 4.5m
                    }
                }
            };

            foreach (var user in helperUsers)
            {
                var result = await userManager.CreateAsync(user, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, RoleConstants.Helper);
                    await userManager.AddToRoleAsync(user, RoleConstants.Admin);
                }
            }

            return helperUsers;
        }
        

        private static async Task SeedPostsAsync(ApplicationDbContext context, List<ApplicationUser> patientUsers)
        {
            if (!context.Posts.Any())
            {
                var posts = new List<Post>
                {
                    new Post
                    {
                        UserId = patientUsers[0].Id,
                        Content = "Looking for support with depression",
                        CreatedAt = DateTime.UtcNow.AddDays(-3),
                        Status = PostStatus.Pending
                    },
                    new Post
                    {
                        UserId = patientUsers[1].Id,
                        Content = "Need assistance with stress management",
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        Status = PostStatus.Pending
                    },
                    new Post
                    {
                        UserId = patientUsers[2].Id,
                        Content = "Looking for support with depression",
                        CreatedAt = DateTime.UtcNow.AddDays(-3),
                        Status = PostStatus.Pending
                    },
                    new Post
                    {
                        UserId = patientUsers[3].Id,
                        Content = "Need assistance with stress management",
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        Status = PostStatus.Pending
                    },
                    new Post
                    {
                        UserId = patientUsers[4].Id,
                        Content = "Seeking help with anxiety issues",
                        CreatedAt = DateTime.UtcNow.AddDays(-1),
                        Status = PostStatus.Pending
                    }
                };

                await context.Posts.AddRangeAsync(posts);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedPostReactionsAsync(ApplicationDbContext context, List<ApplicationUser> helperUsers)
        {
            if (!context.Set<PostReaction>().Any())
            {
                var posts = await context.Posts.ToListAsync();
                if (posts.Any() && helperUsers.Any())
                {
                    var reactions = new List<PostReaction>
                    {
                        new PostReaction
                        {
                            PostId = posts[0].Id,
                            AcceptorId = helperUsers[0].Id,
                            CreatedAt = DateTime.UtcNow.AddHours(-6)
                        },
                        new PostReaction
                        {
                            PostId = posts[1].Id,
                            AcceptorId = helperUsers[1].Id,
                            CreatedAt = DateTime.UtcNow.AddHours(-4)
                        },
                        
                    };

                    await context.Set<PostReaction>().AddRangeAsync(reactions);

                    // Update post status for reacted posts
                    posts[3].Status = PostStatus.Accepted;
                    posts[4].Status = PostStatus.Accepted;

                    await context.SaveChangesAsync();
                }
            }
        }

        // seed notifications
        private static async Task SeedNotificationsAsync(ApplicationDbContext context)
        {
            if (!context.Set<Notification>().Any())
            {
                var users = await context.Users.ToListAsync();
                var posts = await context.Posts.ToListAsync();
                if (users.Count >= 4 && posts.Any())
                {
                    var notifications = new List<Notification>
                    {
                        new Notification
                        {
                            UserId = users[0].Id,
                            RecipientId = users[1].Id,
                            Content = "New post available for help",
                            Type = "Post",
                            CreatedAt = DateTime.UtcNow.AddHours(-2),
                            IsRead = false
                        },
                        new Notification
                        {
                            UserId = users[1].Id,
                            RecipientId = users[2].Id,
                            Content = "New post available for help",
                            Type = "Post",
                            CreatedAt = DateTime.UtcNow.AddHours(-1),
                            IsRead = false
                        }
                    };
                    await context.Set<Notification>().AddRangeAsync(notifications);
                    await context.SaveChangesAsync();
                }
            }
        }
        private static async Task SeedCallHistoryAsync(ApplicationDbContext context)
        {
            if (!context.Set<CallHistory>().Any())
            {
                var users = await context.Users.ToListAsync();
                var posts = await context.Posts.ToListAsync();

                if (users.Count >= 5 && posts.Count >= 5)
                {
                    // Create a variety of call history records with different statuses, durations, and scenarios
                    var callHistories = new List<CallHistory>
            {
                // Completed calls
                //  make the caller is the patient and the callee is the helper
               
                new CallHistory
                {
                    PostId = posts[0].Id,
                    CallerId = users[1].Id,
                    CalleeId = users[2].Id,
                    StartTime = DateTime.UtcNow.AddDays(-7),
                    EndTime = DateTime.UtcNow.AddDays(-7).AddMinutes(10),
                    Status = CallStatus.Ended,
                    RoomName = $"room-{posts[0].Id}",
                    LastHeartbeat = DateTime.UtcNow.AddDays(-7).AddMinutes(10)
                },
                new CallHistory
                {
                    PostId = posts[0].Id,
                    CallerId = users[0].Id,
                    CalleeId = users[3].Id, // Helper
                    StartTime = DateTime.UtcNow.AddDays(-5),
                    EndTime = DateTime.UtcNow.AddDays(-5).AddMinutes(25),
                    Status = CallStatus.Ended,
                    RoomName = $"room-{posts[0].Id}",
                    LastHeartbeat = DateTime.UtcNow.AddDays(-5).AddMinutes(25)
                },
                new CallHistory
                {
                    PostId = posts[1].Id,
                    CallerId = users[3].Id, // Helper initiated
                    CalleeId = users[1].Id,
                    StartTime = DateTime.UtcNow.AddDays(-3),
                    EndTime = DateTime.UtcNow.AddDays(-3).AddMinutes(15),
                    Status = CallStatus.Ended,
                    RoomName = $"room-{posts[1].Id}",
                    LastHeartbeat = DateTime.UtcNow.AddDays(-3).AddMinutes(15)
                },
                
                // Longer call
                new CallHistory
                {
                    PostId = posts[2].Id,
                    CallerId = users[1].Id,
                    CalleeId = users[4].Id,
                    StartTime = DateTime.UtcNow.AddDays(-2),
                    EndTime = DateTime.UtcNow.AddDays(-2).AddMinutes(45),
                    Status = CallStatus.Ended,
                    RoomName = $"room-{posts[2].Id}",
                    LastHeartbeat = DateTime.UtcNow.AddDays(-2).AddMinutes(45)
                },
                
                // Call that was disconnected abnormally
                new CallHistory
                {
                    PostId = posts[3].Id,
                    CallerId = users[2].Id,
                    CalleeId = users[3].Id,
                    StartTime = DateTime.UtcNow.AddDays(-1).AddHours(-5),
                    EndTime = DateTime.UtcNow.AddDays(-1).AddHours(-4).AddMinutes(12),
                    Status = CallStatus.Disconnected,
                    DisconnectReason = "Call automatically disconnected due to inactivity",
                    RoomName = $"room-{posts[3].Id}",
                    LastHeartbeat = DateTime.UtcNow.AddDays(-1).AddHours(-4).AddMinutes(7)
                },
                
                // Very recent call
                new CallHistory
                {
                    PostId = posts[4].Id,
                    CallerId = users[0].Id,
                    CalleeId = users[4].Id,
                    StartTime = DateTime.UtcNow.AddHours(-4),
                    EndTime = DateTime.UtcNow.AddHours(-3).AddMinutes(22),
                    Status = CallStatus.Ended,
                    RoomName = $"room-{posts[4].Id}",
                    LastHeartbeat = DateTime.UtcNow.AddHours(-3).AddMinutes(22)
                },
                
                // Failed call - never connected
                new CallHistory
                {
                    PostId = posts[0].Id,
                    CallerId = users[2].Id,
                    CalleeId = users[3].Id,
                    StartTime = DateTime.UtcNow.AddHours(-10),
                    EndTime = DateTime.UtcNow.AddHours(-10).AddMinutes(2),
                    Status = CallStatus.Failed,
                    DisconnectReason = "Call failed to connect",
                    RoomName = $"room-{posts[0].Id}-retry",
                    LastHeartbeat = DateTime.UtcNow.AddHours(-10).AddMinutes(2)
                },
                
                // Short call
                new CallHistory
                {
                    PostId = posts[1].Id,
                    CallerId = users[1].Id,
                    CalleeId = users[4].Id,
                    StartTime = DateTime.UtcNow.AddDays(-1).AddHours(-2),
                    EndTime = DateTime.UtcNow.AddDays(-1).AddHours(-2).AddMinutes(5),
                    Status = CallStatus.Ended,
                    RoomName = $"room-{posts[1].Id}-second",
                    LastHeartbeat = DateTime.UtcNow.AddDays(-1).AddHours(-2).AddMinutes(5)
                },
                
                // Ongoing call (still connected)
                new CallHistory
                {
                    PostId = posts[2].Id,
                    CallerId = users[0].Id,
                    CalleeId = users[3].Id,
                    StartTime = DateTime.UtcNow.AddMinutes(-15),
                    EndTime = null,
                    Status = CallStatus.Connected,
                    RoomName = $"room-{posts[2].Id}-current",
                    LastHeartbeat = DateTime.UtcNow.AddMinutes(-2)
                },
                
                // Call initiated but not yet connected (callee hasn't joined)
                new CallHistory
                {
                    PostId = posts[3].Id,
                    CallerId = users[2].Id,
                    CalleeId = users[4].Id,
                    StartTime = DateTime.UtcNow.AddMinutes(-5),
                    EndTime = null,
                    Status = CallStatus.Initiated,
                    RoomName = $"room-{posts[3].Id}-waiting",
                    LastHeartbeat = DateTime.UtcNow.AddMinutes(-5)
                },
                
                // Historic call from last week
                new CallHistory
                {
                    PostId = posts[4].Id,
                    CallerId = users[1].Id,
                    CalleeId = users[3].Id,
                    StartTime = DateTime.UtcNow.AddDays(-7),
                    EndTime = DateTime.UtcNow.AddDays(-7).AddMinutes(33),
                    Status = CallStatus.Ended,
                    RoomName = $"room-{posts[4].Id}-old",
                    LastHeartbeat = DateTime.UtcNow.AddDays(-7).AddMinutes(33)
                }
            };

                    await context.Set<CallHistory>().AddRangeAsync(callHistories);
                    await context.SaveChangesAsync();

                    Console.WriteLine($"Seeded {callHistories.Count} call history records");
                }
                else
                {
                    Console.WriteLine("Not enough users or posts to seed call history data");
                }
            }
            else
            {
                Console.WriteLine("Call history records already exist - skipping seed");
            }
        }
    }
}
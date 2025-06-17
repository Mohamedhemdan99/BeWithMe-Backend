using System.Text.RegularExpressions;
using BeWithMe.Models;
using BeWithMe.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BeWithMe.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() { }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Helper> Helpers { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<hubUserGroup> hubUserGroups { get; set; }
        public DbSet<hubUserConnections> hubUserConnections { get; set; }
        public DbSet<hubGroup> hubGroups { get; set; }
        public DbSet<PostReaction> PostReactions { get; set; }
        public DbSet<CallHistory> CallHistories { get; set; }
        #region oldcreating

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    // Configure TPH inheritance
        //    modelBuilder.Entity<Profile>()
        //   .ToTable("Profiles")
        //   .HasDiscriminator<string>("UserType")
        //   .HasValue<PatientProfile>("Patient")
        //   .HasValue<HelperProfile>("Helper");

        //    // 1️⃣ Configure the relationship explicitly
        //    modelBuilder.Entity<Profile>()
        //        .HasOne(p => p.User)
        //        .WithMany() // If ApplicationUser has a navigation property, use: .WithMany(u => u.Profiles)
        //        .HasForeignKey(p => p.UserId)
        //        .IsRequired()
        //        .OnDelete(DeleteBehavior.Cascade);

        //    // 2️⃣ Remove any auto-generated indexes on UserId
        //    var profileEntity = modelBuilder.Entity<Profile>();
        //    var existingIndexes = profileEntity.Metadata.GetIndexes()
        //        .Where(i => i.Properties.Any(p => p.Name == "UserId"))
        //        .ToList();
        //    foreach (var index in existingIndexes)
        //    {
        //        profileEntity.Metadata.RemoveIndex(index);
        //    }

        //    // 3️⃣ Add your unique index with an explicit name
        //    modelBuilder.Entity<Profile>()
        //        .HasIndex(p => p.UserId)
        //        .HasDatabaseName("IX_Profiles_UserId_Unique")
        //        .IsUnique();

        //    // Configure one-to-many relationships
        //    modelBuilder.Entity<ApplicationUser>()
        //        .HasMany(u => u.Notifications)
        //        .WithOne(n => n.User)
        //        .HasForeignKey(n => n.UserId);

        //    modelBuilder.Entity<Helper>()
        //        .HasMany(h => h.ProvidedHelp)
        //        .WithOne(hi => hi.Helper)
        //        .HasForeignKey(hi => hi.AcceptorId);

        //    modelBuilder.Entity<Patient>()
        //        .HasMany(p => p.ReceivedHelp)
        //        .WithOne(hi => hi.Patient)
        //        .HasForeignKey(hi => hi.PatientId);

        //    // Indexes for performance
        //    modelBuilder.Entity<Notification>()
        //        .HasIndex(n => n.UserId);

        //    modelBuilder.Entity<HelpInstance>()
        //        .HasIndex(hi => new { hi.AcceptorId, hi.PatientId });
        //} 
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Patient>()
         .HasOne(p => p.User)
         .WithOne(u => u.Patient)
         .HasForeignKey<Patient>(p => p.UserId);

            modelBuilder.Entity<Helper>()
                .HasOne(h => h.User)
                .WithOne(u => u.Helper)
                .HasForeignKey<Helper>(h => h.UserId);

           

            // Configure Post Status Enum to store as string
            modelBuilder.Entity<Post>()
                .Property(p => p.Status)
                .HasConversion<string>();

            // Indexes for performance
            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.UserId);
            
            // Configure composite primary key for UserGroup
            modelBuilder.Entity<hubUserGroup>()
                .HasKey(ug => new { ug.UserId, ug.GroupId });

            // Ensure GroupName is unique
            modelBuilder.Entity<hubGroup>()
                .HasIndex(g => g.GroupName)
                .IsUnique();
        }
    }
}


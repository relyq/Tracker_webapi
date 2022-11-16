using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tracker.Models;

namespace Tracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<
        ApplicationUser, 
        IdentityRole, string, 
        IdentityUserClaim<string>, 
        UserRole, 
        IdentityUserLogin<string>, 
        IdentityRoleClaim<string>, 
        IdentityUserToken<string>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Ticket> Ticket { get; set; }
        public DbSet<TicketStatus> TicketStatus { get; set; }
        public DbSet<TicketType> TicketType { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<Comment> Comment { get; set; }
        public DbSet<Organization> Organization { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId, ur.OrganizationId });

            builder.Entity<Project>()
                .HasOne(p => p.Author)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Ticket>()
                .HasOne(t => t.Assignee)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Entity<Ticket>()
                .HasOne(t => t.Submitter)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);
            
            builder.Entity<Ticket>()
                .HasOne(t => t.Type)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Ticket>()
                .HasOne(t => t.Status)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Comment>()
                .HasOne(c => c.Ticket)
                .WithMany(t => t.Comments)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comment>()
                .HasOne(c => c.Author)
                .WithMany()
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ApplicationUser>()
                .Property(u => u.Created)
                .HasDefaultValueSql("GETUTCDATE()");
            
            builder.Entity<Organization>()
                .Property(o => o.Created)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<Project>()
                .Property(p => p.Created)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<Ticket>()
                .Property(t => t.Created)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<Comment>()
                .Property(c => c.Created)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<Organization>()
                .Property(o => o.Id)
                .HasDefaultValueSql("NEWID()");

            builder.ApplyConfiguration(new RoleConfiguration());
        }
    }
}
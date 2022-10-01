using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tracker.Models;

namespace Tracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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

            builder.ApplyConfiguration(new RoleConfiguration());
        }
    }
}
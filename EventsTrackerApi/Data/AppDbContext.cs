using EventsTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        // Tablas en la base de datos
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventInvitation> EventInvitations { get; set; }
        public DbSet<EventPost> EventPosts { get; set; }        
        public DbSet<Location> Location { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar las relaciones entre User y EventInvitation

            // modelBuilder.HasSequence<int>("DniSequence")
            //    .StartsAt(1)
            //    .IncrementsBy(1);

            modelBuilder.Entity<EventInvitation>()
                .HasOne(ei => ei.User)
                .WithMany(u => u.ReceivedInvitations)
                .HasForeignKey(ei => ei.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventInvitation>()
                .HasOne(ei => ei.Creator)
                .WithMany(u => u.CreatedInvitations)
                .HasForeignKey(ei => ei.CreatorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventInvitation>()
                .HasOne(ei => ei.Event)
                .WithMany(e => e.Invitations)
                .HasForeignKey(ei => ei.EventID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

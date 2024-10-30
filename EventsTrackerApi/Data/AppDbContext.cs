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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de las relaciones de Event
            modelBuilder.Entity<Event>() // Con user creador
                .HasOne(e => e.Creator)
                .WithMany(u => u.CreatedEvents)
                .HasForeignKey(e => e.CreatorID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de las relaciones de EventInvitation
            modelBuilder.Entity<EventInvitation>() // Con Event
                .HasOne(ei => ei.Event)
                .WithMany(e => e.Invitations)
                .HasForeignKey(ei => ei.EventID);

            modelBuilder.Entity<EventInvitation>() // Con User invitado
                .HasOne(ei => ei.User)
                .WithMany(u => u.ReceivedInvitations)
                .HasForeignKey(ei => ei.UserID);

            modelBuilder.Entity<EventInvitation>() // Con User creador
                .HasOne(ei => ei.User)
                .WithMany(u => u.CreatedInvitations)
                .HasForeignKey(ei => ei.CreatorID);

            // Configuración de las relaciones de EventPost
            modelBuilder.Entity<EventPost>()
                .HasOne(ep => ep.Event)
                .WithMany(e => e.Posts)
                .HasForeignKey(ep => ep.EventID);

            modelBuilder.Entity<EventPost>()
                .HasOne(ep => ep.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(ep => ep.UserID);
        }
    }
}

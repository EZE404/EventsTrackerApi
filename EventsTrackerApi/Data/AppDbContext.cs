using Microsoft.EntityFrameworkCore;
using MyProject.Models;

namespace MyProject.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Tablas en la base de datos
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventStatus> EventStatuses { get; set; }
        public DbSet<EventInvitation> EventInvitations { get; set; }
        public DbSet<EventPost> EventPosts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de relaciones muchos a muchos entre User y Role
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserID, ur.RoleID });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserID);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleID);

            // Configuración de la relación uno a muchos entre User y Event
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Creator)
                .WithMany(u => u.Events)
                .HasForeignKey(e => e.CreatorID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de la relación uno a muchos entre EventStatus y Event
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Status)
                .WithMany(es => es.Events)
                .HasForeignKey(e => e.StatusID);

            // Configuración de la relación entre Event y EventInvitation
            modelBuilder.Entity<EventInvitation>()
                .HasOne(ei => ei.Event)
                .WithMany(e => e.EventInvitations)
                .HasForeignKey(ei => ei.EventID);

            modelBuilder.Entity<EventInvitation>()
                .HasOne(ei => ei.User)
                .WithMany(u => u.EventInvitations)
                .HasForeignKey(ei => ei.UserID);

            // Configuración de la relación entre Event y EventPost
            modelBuilder.Entity<EventPost>()
                .HasOne(ep => ep.Event)
                .WithMany(e => e.EventPosts)
                .HasForeignKey(ep => ep.EventID);

            modelBuilder.Entity<EventPost>()
                .HasOne(ep => ep.User)
                .WithMany(u => u.EventPosts)
                .HasForeignKey(ep => ep.UserID);
        }
    }
}

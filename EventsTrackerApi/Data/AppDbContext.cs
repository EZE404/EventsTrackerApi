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
        public DbSet<Tag> Tags { get; set; }
        public DbSet<EventTag> EventTags { get; set; }
        public DbSet<EventRating> EventRatings { get; set; }

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

            // Evitar invitaciones duplicadas: un usuario no puede tener más de una invitación por evento
            modelBuilder.Entity<EventInvitation>(e =>
            {
                e.HasIndex(x => new { x.EventID, x.UserID }).IsUnique();
            });

            modelBuilder.Entity<Tag>(e =>
            {
                e.ToTable("Tags");
                e.Property(x => x.Name).HasMaxLength(80).IsRequired();
                e.HasIndex(x => x.Name).IsUnique(); // con utf8mb4_* será case-insensitive
            });

            modelBuilder.Entity<EventTag>(e =>
            {
                e.ToTable("EventTags");
                e.HasKey(x => new { x.EventId, x.TagId });     // PK compuesta

                e.HasOne(x => x.Event)
                    .WithMany(ev => ev.EventTags)
                    .HasForeignKey(x => x.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Tag)
                    .WithMany(t => t.EventTags)
                    .HasForeignKey(x => x.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<EventRating>(e =>
            {
                e.ToTable("EventRatings");
                e.HasKey(x => new { x.EventId, x.UserId });
                e.Property(x => x.Score).IsRequired();

                e.HasOne(x => x.Event)
                .WithMany()
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

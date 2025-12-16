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
        public DbSet<UserDeviceToken> UserDeviceTokens { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<UserImage> UserImages => Set<UserImage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- INICIO: Configuración para EventPost ---
            modelBuilder.Entity<EventPost>(e =>
            {
                e.ToTable("EventPosts");

                // Relación con User: Un usuario puede tener muchos posts.
                // Si se elimina el usuario, sus posts se eliminan en cascada.
                e.HasOne(p => p.User)
                    .WithMany(u => u.Posts)
                    .HasForeignKey(p => p.UserID)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación con Event: Un evento puede tener muchos posts.
                // Si se elimina el evento, sus posts se eliminan en cascada.
                e.HasOne(p => p.Event)
                    .WithMany(ev => ev.Posts)
                    .HasForeignKey(p => p.EventID)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            // --- FIN: Configuración para EventPost ---

            // --- INICIO: Configuración para Invitaciones ---

            // Configura la conversión del enum InvitationStatus a string para la base de datos.
            modelBuilder.Entity<EventInvitation>()
                .Property(e => e.ResponseStatus)
                .HasConversion<string>();

            // Configurar las relaciones entre User y EventInvitation
            modelBuilder.Entity<EventInvitation>()
                .HasOne(ei => ei.User)
                .WithMany(u => u.ReceivedInvitations)
                .HasForeignKey(ei => ei.UserId) // Corregido
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventInvitation>()
                .HasOne(ei => ei.Creator)
                .WithMany(u => u.CreatedInvitations)
                .HasForeignKey(ei => ei.CreatorId) // Corregido
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventInvitation>()
                .HasOne(ei => ei.Event)
                .WithMany(e => e.Invitations)
                .HasForeignKey(ei => ei.EventId) // Corregido
                .OnDelete(DeleteBehavior.Cascade);

            // Evitar invitaciones duplicadas: un usuario no puede tener más de una invitación por evento
            modelBuilder.Entity<EventInvitation>(e =>
            {
                e.HasIndex(x => new { x.EventId, x.UserId }).IsUnique(); // Corregido
            });

            // --- FIN: Configuración para Invitaciones ---

            modelBuilder.Entity<Tag>(e =>
            {
                e.ToTable("Tags");
                e.Property(x => x.Name).HasMaxLength(80).IsRequired();
                e.HasIndex(x => x.Name).IsUnique();
            });

            modelBuilder.Entity<EventTag>(e =>
            {
                e.ToTable("EventTags");
                e.HasKey(x => new { x.EventId, x.TagId });

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

            modelBuilder.Entity<UserDeviceToken>(e =>
            {
                e.ToTable("UserDeviceTokens");

                e.Property(x => x.Token).HasMaxLength(255).IsRequired();
                e.Property(x => x.Platform).HasMaxLength(20).IsRequired();
                e.Property(x => x.DeviceId).HasMaxLength(128);
                e.Property(x => x.AppVersion).HasMaxLength(20);

                e.Property(x => x.IsActive).HasDefaultValue(true);
                e.Property(x => x.LastSeenUtc).HasDefaultValueSql("UTC_TIMESTAMP()");
                e.Property(x => x.CreatedAtUtc).HasDefaultValueSql("UTC_TIMESTAMP()");
                e.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("UTC_TIMESTAMP()");

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.Token).IsUnique();

                e.HasIndex(x => new { x.UserId, x.DeviceId }).IsUnique();
            });

            modelBuilder.Entity<User>()
            .HasOne(u => u.Avatar)
            .WithOne(i => i.User)
            .HasForeignKey<UserImage>(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<UserImage>(e =>
            {
                e.Property(p => p.ContentType).HasMaxLength(100).IsRequired();
                e.Property(p => p.Data).IsRequired();
                e.Property(p => p.Length);
                e.Property(p => p.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
                e.HasIndex(p => p.UpdatedAt);
            });

            // --- INICIO: Configuración para Favorite ---
            modelBuilder.Entity<Favorite>(e =>
            {
                e.ToTable("Favorites");
                e.HasKey(x => new { x.EventId, x.UserId });

                e.HasOne(x => x.Event)
                    .WithMany()
                    .HasForeignKey(x => x.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.Property(x => x.CreatedAtUtc).IsRequired();
                e.HasIndex(x => x.UserId);
                e.HasIndex(x => x.EventId);
            });
            // --- FIN: Configuración para Favorite ---
        }
    }
}

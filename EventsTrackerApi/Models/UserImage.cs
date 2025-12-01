using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventsTrackerApi.Models
{
    // Guarda el binario del avatar del usuario
    public class UserImage
    {
        // PK = FK a User.ID (relación 1:1)
        [Key, ForeignKey(nameof(User))]
        public int UserId { get; set; }

        // MIME type ("image/jpeg", "image/png", etc.)
        [Required, StringLength(100)]
        public string ContentType { get; set; } = "image/jpeg";

        // Tamaño en bytes (solo informativo/validación)
        public int Length { get; set; }

        // Blob
        [Required]
        public byte[] Data { get; set; } = [];

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navegación inversa
        public User? User { get; set; }
    }
}

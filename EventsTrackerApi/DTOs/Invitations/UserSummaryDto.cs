namespace EventsTrackerApi.DTOs.Invitations
{
    /// <summary>
    /// DTO que representa un resumen de la información de un usuario,
    /// utilizado dentro de otros DTOs como el de invitaciones.
    /// </summary>
    public class UserSummaryDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; }
        public string? Email { get; set; }
    }
}

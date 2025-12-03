using System.Collections.Generic;

namespace EventsTrackerApi.DTOs.Invitations
{
    public class ValidateEmailsRequest
    {
        public List<string> Emails { get; set; } = new();
    }

    public class ValidateEmailsResponse
    {
        public List<ValidatedUserDto> ValidUsers { get; set; } = new();
        public List<string> InvalidEmails { get; set; } = new();
    }

    /// <summary>
    /// DTO que representa un usuario validado durante el proceso de invitación.
    /// Esta definición se movió aquí para resolver un error de compilación (CS0246).
    /// </summary>
    public class ValidatedUserDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public bool AlreadyInvited { get; set; }
    }
}

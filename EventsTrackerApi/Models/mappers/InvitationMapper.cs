using EventsTrackerApi.DTOs.Invitations;
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Models.mappers;

public static class InvitationMapper
{
    public static EventInvitationDto ToDto(EventInvitation ei)
    {
        return new EventInvitationDto
        {
            Id = ei.ID,
            EventID = ei.EventID,
            ReceiverID = ei.UserID,
            SenderID = ei.CreatorID,
            ResponseStatus = NormalizeStatusForClient(ei.ResponseStatus),
            SentDate = DateTime.SpecifyKind(ei.SentDate, DateTimeKind.Utc),
            ResponseDate = ei.ResponseDate.HasValue ? DateTime.SpecifyKind(ei.ResponseDate.Value, DateTimeKind.Utc) : null,
            Sender = ToUserLite(ei.Creator),
            Receiver = ToUserLite(ei.User),
            InvitedUser = new ValidatedUserDto
            {
                Id = ei.User?.ID ?? ei.UserID,
                NombreCompleto = ei.User?.NombreCompleto() ?? string.Empty,
                Email = ei.User?.Email ?? string.Empty,
                AlreadyInvited = true
            },
            InvitedBy = new ValidatedUserDto
            {
                Id = ei.Creator?.ID ?? ei.CreatorID,
                NombreCompleto = ei.Creator?.NombreCompleto() ?? string.Empty,
                Email = ei.Creator?.Email ?? string.Empty,
                AlreadyInvited = false
            },
            Event = ei.Event != null ? new EventLiteDto { Id = ei.Event.ID, Name = ei.Event.Name } : null,
            Status = NormalizeStatusForClient(ei.ResponseStatus)
        };
    }

    public static UserLiteDto? ToUserLite(User? u) => u == null ? null : new UserLiteDto
    {
        Id = u.ID,
        Dni = u.Dni,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Email = u.Email,
        Direccion = u.Direccion,
        TelefonoArea = u.TelefonoArea,
        TelefonoNumero = u.TelefonoNumero,
        IsHost = u.IsHost
    };

    // Mapea valores almacenados a etiquetas que espera la UI (es/EN)
    public static string NormalizeStatusForClient(string? status)
    {
        var s = status?.Trim().ToUpperInvariant();
        return s switch
        {
            "PENDING" => "Pendiente",
            "ACCEPTED" => "Aceptada",
            "REJECTED" => "Rechazada",
            "PENDIENTE" => "Pendiente",
            "ACEPTADA" => "Aceptada",
            "RECHAZADA" => "Rechazada",
            _ => "Pendiente"
        };
    }

    // Convierte input de cliente a forma canónica que guardamos
    public static string NormalizeStatusForStorage(string? status)
    {
        var s = status?.Trim().ToUpperInvariant();
        return s switch
        {
            "ACEPTADA" => "ACCEPTED",
            "RECHAZADA" => "REJECTED",
            "PENDIENTE" => "PENDING",
            "ACCEPTED" => "ACCEPTED",
            "REJECTED" => "REJECTED",
            _ => "PENDING"
        };
    }
}

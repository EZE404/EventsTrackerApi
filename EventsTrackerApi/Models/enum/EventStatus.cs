using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace EventsTrackerApi.Models;

/// <summary>
/// Defines the possible statuses of an event.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventStatus
{
    /// <summary>
    /// Event is in draft mode.
    /// </summary>
    [EnumMember(Value = "BORRADOR")]
    BORRADOR = 0,

    /// <summary>
    /// Event is published and visible to users.
    /// </summary>
    [EnumMember(Value = "PUBLICADO")]
    PUBLICADO = 1,

    /// <summary>
    /// Event has concluded.
    /// </summary>
    [EnumMember(Value = "CONCLUIDO")]
    CONCLUIDO = 2,

    /// <summary>
    /// Event has been cancelled.
    /// </summary>
    [EnumMember(Value = "CANCELADO")]
    CANCELADO = 3
}
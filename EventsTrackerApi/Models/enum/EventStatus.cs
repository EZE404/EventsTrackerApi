using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace EventsTrackerApi.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventStatus
{
    [EnumMember(Value = "BORRADOR")]
    BORRADOR = 0,
    [EnumMember(Value = "PUBLICADO")]
    PUBLICADO = 1,
    [EnumMember(Value = "CONCLUIDO")]
    CONCLUIDO = 2,
    [EnumMember(Value = "CANCELADO")]
    CANCELADO = 3
}
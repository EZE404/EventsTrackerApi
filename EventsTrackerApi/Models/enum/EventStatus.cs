using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace EventsTrackerApi.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventStatus
{
    [EnumMember(Value = "PUBLICADO")]
    PUBLICADO = 0,

    [EnumMember(Value = "CONCLUIDO")]
    CONCLUIDO = 1,

    [EnumMember(Value = "CANCELADO")]
    CANCELADO = 2,

    [EnumMember(Value = "BORRADOR")]
    BORRADOR = 3
}
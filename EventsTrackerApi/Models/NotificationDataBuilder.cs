using System.Collections.ObjectModel;
using System.Globalization;

namespace EventsTrackerApi.Models;

public static class NotificationDataBuilder
{
    private const string DefaultChannel = "general_channel";
    private const string AppScheme = "eventstracker-url"; // ok con guión y minúsculas

    public static IReadOnlyDictionary<string, string> Build(
        NotificationAction action,
        Event? e = null,
        IDictionary<string, string>? extra = null)
    {
        var data = CreateBase();

        switch (action)
        {
            case NotificationAction.EventExpired:
                RequireEvent(e, action);
                AddEvent(data, e!);
                data["action"] = "event_expired";
                data["title"]  = $"El evento '{e!.Name}' finaliza pronto";
                data["body"]   = $"Finaliza el {e!.EndDateTime:dd/MM/yyyy HH:mm}";
                break;

            case NotificationAction.EventStartingSoon:
                RequireEvent(e, action);
                AddEvent(data, e!);
                data["action"]   = "event_starting_soon";
                data["title"]    = $"El evento '{e!.Name}' empieza pronto";
                data["body"]     = $"Empieza el {e!.StartDateTime:dd/MM/yyyy HH:mm}";
                data["startUtc"] = e!.StartDateTime.ToString("o", CultureInfo.InvariantCulture);
                break;

            case NotificationAction.EventUpdated:
                RequireEvent(e, action);
                AddEvent(data, e!);
                data["action"] = "event_updated";
                data["title"]  = $"Se actualizó '{e!.Name}'";
                data["body"]   = "Revisá los cambios del evento";
                break;

            case NotificationAction.EventsExpiringTomorrow:
                data["action"] = "events_expiring_tomorrow";
                data["entity"] = "event_list";
                    if (extra == null || !extra.TryGetValue("count", out var count) || string.IsNullOrWhiteSpace(count))
                    throw new ArgumentException("EventsExpiringTomorrow requiere 'count' en extra.");
                data["title"]   = $"Tienes {count} evento{(count == "1" ? "" : "s")} que vencen mañana";
                data["body"]    = "Toca para ver la lista completa.";
                if (extra.TryGetValue("eventIds", out var eventIds))   data["eventIds"]   = eventIds;
                if (extra.TryGetValue("preview", out var preview))     data["preview"]    = preview;
                if (extra.TryGetValue("deeplinkList", out var dl))     data["deeplink"]   = dl; // opcional
                data["collapseId"] = "events_tomorrow_summary"; // colapsa múltiples envíos iguales
                break;
            case NotificationAction.Profile:
                data["action"] = "profile";
                data["entity"] = "user";
                if (extra != null && extra.TryGetValue("userId", out var uid)) data["userId"] = uid;
                break;

            case NotificationAction.SyncMeetings:
                data["action"] = "sync_meetings";
                data["entity"] = "system";
                break;

            default:
                data["action"] = "noop";
                data["entity"] = "system";
                break;
        }

        if (extra != null)
            foreach (var kv in extra) data[kv.Key] = kv.Value;

        return new ReadOnlyDictionary<string, string>(data);
    }

    // ---------- helpers ----------
    private static Dictionary<string, string> CreateBase() => new(StringComparer.Ordinal)
    {
        ["v"]       = "1",
        ["msgId"]   = Guid.NewGuid().ToString("N"),
        ["ts"]      = DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture),
        ["channel"] = DefaultChannel
    };

    private static void AddEvent(Dictionary<string, string> data, Event e)
    {
        data["entity"]    = "event";
        data["entityId"]  = e.ID.ToString();
        data["name"]      = e.Name ?? string.Empty;
        data["endUtc"]    = e.EndDateTime.ToString("o", CultureInfo.InvariantCulture);
        data["deeplink"]  = $"{AppScheme}://events/{e.ID}";
        data["collapseId"]= $"event_{e.ID}_status";
    }

    private static void RequireEvent(Event? e, NotificationAction action)
    {
        if (e is null) throw new ArgumentNullException(nameof(e), $"'{action}' requiere un Event.");
    }
}
using System.Collections.Concurrent;
using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using EventsTrackerApi.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventsTrackerApi.Job;

public class EventsForDefeatJob(
            ILogger<EventsForDefeatJob> logger,
            FcmService fcm,
            IOptions<NotificationsOptions> opts,
            IServiceScopeFactory scopeFactory
    ) : BackgroundService
{
    private readonly ILogger<EventsForDefeatJob> _logger = logger;
    private readonly FcmService _fcm = fcm;
    private readonly IOptions<NotificationsOptions> _opts = opts;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly int MINUTES_TO_SYNC = 60; // cada 60 minutos

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(MINUTES_TO_SYNC));
        await RunJob(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            _logger.LogInformation("RunJob tick: nowUtc={Now:u}, minutes={Min}", DateTime.UtcNow, MINUTES_TO_SYNC);

            await RunJob(stoppingToken);
        }
    }

    private const int MaxDegreeOfParallelism = 12;

    private static (DateTime startUtc, DateTime endUtc) GetLocalDayUtcRange(TimeZoneInfo tz, DateTime utcNow, int daysOffset)
    {
        var localBase = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz).Date.AddDays(daysOffset);
        var startLocal = localBase;
        var endLocal = localBase.AddDays(1);
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz);
        return (startUtc, endUtc);
    }

    private async Task RunJob(CancellationToken ct)
    {
        try
        {
            var tzId = _opts.Value.TimeZoneId ?? "America/Argentina/San_Luis";
            var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            var (startUtc, endUtc) = GetLocalDayUtcRange(tz, DateTime.UtcNow, daysOffset: 1);

            _logger.LogInformation("Buscando eventos que finalizan entre {Start} y {End} (mañana local).",
                startUtc.ToString("u"), endUtc.ToString("u"));

            using var scope = _scopeFactory.CreateScope();
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var deviceRepo = scope.ServiceProvider.GetRequiredService<IDevicesRepository>();
            var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();

            var events = await eventRepo.GetEventsEndingBetweenAsync(startUtc, endUtc, ct);
            if (!events.Any())
            {
                _logger.LogInformation("No hay eventos por vencer mañana.");
                return;
            }
            _logger.LogInformation("Se encontraron {Count} eventos por vencer mañana.", events.Count());
            var eventIds = events.Select(ev => ev.ID).ToArray();
            var evById = events.ToDictionary(x => x.ID);

            var tokens = await deviceRepo
                .FindAsync(d => d.IsActive)
                .Select(d => new { d.UserId, d.Token })
                .Where(t => !string.IsNullOrWhiteSpace(t.Token))
                .ToListAsync(ct);

            if (tokens.Count == 0)
            {
                _logger.LogWarning("No hay dispositivos activos para notificar.");
                return;
            }
            _logger.LogInformation("Se encontraron {Count} tokens de dispositivos activos.", tokens.Count);

            var work = tokens
                .GroupBy(t => t.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Tokens = g.Select(x => x.Token)
                            .Where(tk => !string.IsNullOrWhiteSpace(tk))
                            .Distinct()
                            .ToList(),
                    Events = events
                })
                .Where(x => x.Tokens.Count > 0 && x.Events.Count() > 0)
                .ToList();
            
            if (work.Count == 0)
            {
                _logger.LogWarning("No hay usuarios con tokens válidos para notificar.");
                return;
            }

            var errors = new ConcurrentBag<(int UserId, string Token, Exception Ex)>();

            await Parallel.ForEachAsync(work, new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                CancellationToken = ct
            },
            async (item, tok) =>
            {
                try
                {
                    var count = item.Events.Count();
                    var topTitles = item.Events.OrderBy(ev => ev.EndDateTime)
                                               .Take(5)
                                               .Select(ev => ev.Name)
                                               .ToArray();
                    var eventIdsCsv = string.Join(",", item.Events.Select(ev => ev.ID));

                    var title = $"Tienes {count} evento{(count > 1 ? "s" : "")} que vencen mañana";
                    var body = "Toca para ver la lista completa.";
                    var extras = new Dictionary<string, string>
                    {
                        ["count"]    = count.ToString(),
                        ["eventIds"] = eventIdsCsv,
                        ["preview"]  = string.Join(" • ", topTitles),
                        ["deeplinkList"] = "eventstracker-url://events/expiring-tomorrow"
                    };

                    var dataType = NotificationDataBuilder.Build(NotificationAction.EventsExpiringTomorrow, e: null, extras);
                   /* var data = new Dictionary<string, string>
                    {
                        ["action"] = "EventsExpiringTomorrow",
                        ["count"] = count.ToString(),
                        ["eventIds"] = eventIdsCsv,
                        ["preview"] = string.Join(" • ", topTitles)
                    };*/

                    foreach (var tk in item.Tokens)
                    {
                        try
                        {
                            await _fcm.SendToTokenAsync(tk, title, body, dataType, ct: tok);
                              _logger.LogInformation("FCM OK user={UserId} token={TokenPrefix}", item.UserId, tk[..Math.Min(12, tk.Length)]);

                        }
                        catch (Exception sendEx)
                        {
                            errors.Add((item.UserId, tk, sendEx));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Fallo al preparar notificación para usuario {UserId}.", item.UserId);
                }
            });

            foreach (var err in errors)
                _logger.LogWarning(err.Ex, "Error enviando a user {UserId}, token {Token}", err.UserId, err.Token);

            _logger.LogInformation("Job 'EventosPorVencer' finalizado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en job 'EventosPorVencer'.");
        }
    }

    private static (DateTime startUtc, DateTime endUtc) GetLocalDayUtcRange(TimeZoneInfo tz, DateTime utcNow)
    {
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);
        var startLocal = localNow.Date.AddDays(1);     // 00:00 de mañana, local
        var endLocal = startLocal.AddDays(1);        // 00:00 de pasado mañana, local
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz);
        return (startUtc, endUtc);
    }

}

public class NotificationsOptions
{
    public string? TimeZoneId { get; set; }
}


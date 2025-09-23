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
    private readonly int MINUTES_TO_SYNC = 180;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(MINUTES_TO_SYNC));
        await RunJob(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunJob(stoppingToken);
        }
    }
    /*
        private async Task RunJob(CancellationToken ct)
        {
            try
            {
                // calcular "mañana" en zona horaria local
                var tzId = _opts.Value.TimeZoneId ?? "America/Argentina/San_Luis";
                var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
                var (startUtc, endUtc) = GetLocalDayUtcRange(tz, DateTime.UtcNow);


                _logger.LogInformation("Buscando eventos que finalizan el {FechaLocal} ({FechaUtc} UTC)...",
                    startUtc.ToShortDateString(), endUtc.ToString("u"));

                using var scope = _scopeFactory.CreateScope();
                var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();

                //buscar eventos por vencer
                var events = await eventRepo.GetEventsEndingBetweenAsync(startUtc, endUtc, ct);
                if (!events.Any())
                {
                    _logger.LogInformation("No hay eventos por vencer mañana.");
                    return;
                }

                var sem = new SemaphoreSlim(10);
                //var tasks = events.Select(e => NotificarUsuariosDeEvento(e, ct));
                var tasks = events.Select(async ev =>
                {
                    await sem.WaitAsync(ct);
                    try
                    {
                        await NotificarEventosQueVencenManiana(ev, ct);
                    }
                    finally
                    {
                        sem.Release();
                    }
                });
                await Task.WhenAll(tasks);

                _logger.LogInformation("Job 'EventosPorVencer' finalizado.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en job 'EventosPorVencer'.");
            }
        }

        private async Task NotificarEventosQueVencenManiana(Event e, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var deviceRepo = scope.ServiceProvider.GetRequiredService<IDevicesRepository>();        
            var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
            // Usuarios relacionados a ese evento
            // var users = await _userRepo.GetUsersByEventIdAsync(e.ID);
            //  var users = await userRepo.GetUsersByEventIdAsync(e.ID, ct);

            var eventos = await eventRepo.FindAsync(e => e.EndDateTime >= tomorrowUtc && e.EndDateTime < tomorrowEnd)
            .AsNoTracking()
            .ToListAsync(ct);
            var users = await userRepo.GetAllAsync();
            if (!users.Any())
            {
                _logger.LogInformation("Evento {Id} no tiene usuarios relacionados.", e.ID);
                return;
            }

            var userIds = users.Select(u => u.ID).Distinct().ToArray();

            //TODO: Buscar tokens de los usuarios
            //var tokens = users.Select(u => u.).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
            // var tokens = new List<string>
           // {
                "cfxtbBzfRZSaQohVaBehds:APA91bHq-taWjEnUsjVd10PkIELxJwhlsim3yOz3sGx5N5H987jBD5Omyqhd2XWcEs6WWmkLcTVCd-8oQpA95_qzYRTQ7Ik_QdfohKvu0PBTkPmC-_l3r54"
           // };

            var deviceTokens = await deviceRepo.GetAllAsync();
            var tokens = deviceTokens.Where(t => userIds.Contains(t.UserId)).Select(t => t.Token).ToList(); // await _deviceRepo.GetTokensByUserIdsAsync(userIds, ct);

            // Deduplicar tokens
            var uniqueTokens = new HashSet<string>(tokens.Where(t => !string.IsNullOrWhiteSpace(t)));

            if (uniqueTokens.Count == 0)
            {
                _logger.LogInformation("No hay tokens para notificar en el evento {Id}.", e.ID);
                return;
            }

            _logger.LogInformation("Enviando {Count} notificaciones para evento {Id} ({Name}) con fin {End}.",
                uniqueTokens.Count, e.ID, e.Name, e.EndDateTime);

            var title = $"El evento '{e.Name}' finaliza pronto";
            var body = $"Finaliza el {e.EndDateTime:dd/MM/yyyy HH:mm}";

            var data = NotificationDataBuilder.Build(NotificationAction.EventExpired, e);
            // Envío en paralelo con límite (para no saturar)
            var throttler = new SemaphoreSlim(10);
            var tasks = uniqueTokens.Select(async token =>
            {
                await throttler.WaitAsync(ct);
                try
                {
                    await _fcm.SendToTokenAsync(
                        token,
                        title,
                        body,
                        data,
                        ct: ct
                        );
                }
                catch (Exception sendEx)
                {
                    _logger.LogWarning(sendEx, "Error enviando a token (evento {EventId}).", e.ID);
                }
                finally
                {
                    throttler.Release();
                }
            });

            await Task.WhenAll(tasks);
        }
    */

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

            var eventIds = events.Select(ev => ev.ID).ToArray();
            var evById = events.ToDictionary(x => x.ID);

            var tokens = await deviceRepo
                .FindAsync(d => d.IsActive)
                .Select(d => new { d.UserId, d.Token })
                .Where(t => !string.IsNullOrWhiteSpace(t.Token))
                .ToListAsync(ct);

            if (tokens.Count == 0)
            {
                _logger.LogInformation("No hay dispositivos activos para notificar.");
                return;
            }

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
                _logger.LogInformation("No hay usuarios con tokens válidos para notificar.");
                return;
            }

            var errors = new System.Collections.Concurrent.ConcurrentBag<(int UserId, string Token, Exception Ex)>();

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


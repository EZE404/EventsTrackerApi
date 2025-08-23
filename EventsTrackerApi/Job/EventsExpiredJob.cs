using System.Collections.Concurrent;
using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using EventsTrackerApi.Service;
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
                    await NotificarUsuariosDeEvento(ev, ct);
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

    private async Task NotificarUsuariosDeEvento(Event e, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        // Usuarios relacionados a ese evento
        // var users = await _userRepo.GetUsersByEventIdAsync(e.ID);
        //  var users = await userRepo.GetUsersByEventIdAsync(e.ID, ct);
        var users = await userRepo.GetAllAsync();
        if (!users.Any())
        {
            _logger.LogInformation("Evento {Id} no tiene usuarios relacionados.", e.ID);
            return;
        }

        var userIds = users.Select(u => u.ID).Distinct().ToArray();

        //TODO: Buscar tokens de los usuarios
        //var tokens = await _tokenRepo.GetActiveTokensByUserIdsAsync(userIds);
        var tokens = new List<string>
        {
            "cfxtbBzfRZSaQohVaBehds:APA91bHq-taWjEnUsjVd10PkIELxJwhlsim3yOz3sGx5N5H987jBD5Omyqhd2XWcEs6WWmkLcTVCd-8oQpA95_qzYRTQ7Ik_QdfohKvu0PBTkPmC-_l3r54"
        };

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
                await _fcm.SendToTokenAsync(token, title, body, data, ct: ct);
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


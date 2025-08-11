using System.Collections.Concurrent;
using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using EventsTrackerApi.Service;
using Microsoft.Extensions.Options;

namespace EventsTrackerApi.Job;

public class EventsForDefeatJob : BackgroundService
{
    private readonly ILogger<EventsForDefeatJob> _logger;
    private readonly FcmService _fcm;
    private readonly IOptions<NotificationsOptions> _opts;
    private readonly IServiceScopeFactory _scopeFactory;


    public EventsForDefeatJob(
        ILogger<EventsForDefeatJob> logger,
        FcmService fcm,
        IOptions<NotificationsOptions> opts,
        IServiceScopeFactory scopeFactory
    )
    {
        _logger = logger;
        _fcm = fcm;
        _opts = opts;
        _scopeFactory = scopeFactory; // ¡te faltaba asignarlo!
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
    //    var timer = new PeriodicTimer(TimeSpan.FromHours(1));
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        await RunJob(stoppingToken); // primera pasada

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
            var nowLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
            var tomorrowLocal = nowLocal.Date.AddDays(1);

            // Normalizamos a UTC para consultas (si tu DB guarda UTC)
            var tomorrowUtc = TimeZoneInfo.ConvertTimeToUtc(tomorrowLocal, tz);

            _logger.LogInformation("Buscando eventos que finalizan el {FechaLocal} ({FechaUtc} UTC)...",
                tomorrowLocal.ToShortDateString(), tomorrowUtc.ToString("u"));

            using var scope = _scopeFactory.CreateScope();
            var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
            var userRepo  = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            //buscar eventos por vencer
            var events = await eventRepo.GetEventsEndingOnAsync(tomorrowUtc);
            if (!events.Any())
            {
                _logger.LogInformation("No hay eventos por vencer mañana.");
                return;
            }

            //  por cada evento, buscar usuarios y tokens
            var tasks = events.Select(e => NotificarUsuariosDeEvento(e, eventRepo, ct));
            await Task.WhenAll(tasks);

            _logger.LogInformation("Job 'EventosPorVencer' finalizado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en job 'EventosPorVencer'.");
        }
    }

    private async Task NotificarUsuariosDeEvento(Event e, IEventRepository eventRepo, CancellationToken ct)
    {
        // Usuarios relacionados a ese evento
        // var users = await _userRepo.GetUsersByEventIdAsync(e.ID);
        var users = await eventRepo.GetAllAsync();
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

        // Envío en paralelo con límite (para no saturar)
        var throttler = new SemaphoreSlim(10); // 10 concurrentes
        var tasks = uniqueTokens.Select(async token =>
        {
            await throttler.WaitAsync(ct);
            try
            {
                await _fcm.SendToTokenAsync(token, title, body, ct: ct);
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
}

public class NotificationsOptions
{
    public string? TimeZoneId { get; set; }
}

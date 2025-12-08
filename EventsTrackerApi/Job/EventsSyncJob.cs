using System.Threading.Channels;
using EventsTrackerApi.Models;
using EventsTrackerApi.Service;

namespace EventsTrackerApi.Job;

public class EventsSyncJob : BackgroundService
{
    private readonly ILogger<EventsSyncJob> _logger;
    private readonly FcmService _fcm;
    private readonly int MINUTES_TO_SYNC = 180;

    public EventsSyncJob(ILogger<EventsSyncJob> logger, FcmService fcm)
    {
        _logger = logger;
        _fcm = fcm;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
         _logger.LogInformation("Corriendo cron de eventos...");
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(MINUTES_TO_SYNC));

        await RunCronJob(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunCronJob(stoppingToken);
        }
        throw new NotImplementedException();
    }

    private async Task RunCronJob(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Enviando sync de eventos (cron with token device)...");

            var data = NotificationDataBuilder.Build(NotificationAction.SyncMeetings);

            await _fcm.SendToDivaceTokenAsync(
                deviceToken: "eW2yKeLhSquLb5aCMa5yhF:APA91bGxGXdqqqa5aG1_bS6tyNXMXB_8Up5fXjmW-v2KM1N_BiUMlunLdQoHFpeU-mjFUvDWcDVUPn8bGQSsp_qw7SEW4Z-qhHn8zWc1QyKOhQgsmLhJyRI",
                title: "Sync de eventos",
                body: "Se han sincronizado las eventos, con cron",
                data
            );

            _logger.LogInformation("Notificación enviada al topic 'events'.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando notificación al topic 'events'");
        }
    }
}

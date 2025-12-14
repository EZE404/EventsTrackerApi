using System.Threading.Channels;
using EventsTrackerApi.Models;
using EventsTrackerApi.Service;

namespace EventsTrackerApi.Job;

public class EventsSyncJob : BackgroundService
{
    private readonly ILogger<EventsSyncJob> _logger;
    private readonly FcmService _fcm;
    private readonly int MINUTES_TO_SYNC = 3;

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

            /*await _fcm.SendToDivaceTokenAsync(
                deviceToken: "cWyWQAs8QlSUpa0lyWEw6H:APA91bFdE8aWejd3q1-EA0eRsluSovwxv0IB2K8ePG4vtkByHrwDtb1xq36uBW46w40sAyBZ5sIR7u2QDudd4ZgSsov0GSU56HHPD9yR9PhnwbQClA_0t-U",
                title: "Sync de eventos",
                body: "Se han sincronizado las eventos, con cron",
                data
            );*/

            _logger.LogInformation("Notificación enviada al topic 'events'.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando notificación al topic 'events'");
        }
    }
}

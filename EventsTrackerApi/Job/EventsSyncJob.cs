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
                deviceToken: "fIyHgXtLRUCTm1WoZ1KDB1:APA91bFX3QHNhUzESJgC2f0aMAV44BRCGAQCQ7nQgnpxQMEgBy2JpV8baHcpUmKoppEwC8Z_NuREsgimActIEKq_GtUn9uIOT5UWQysXbnEvmDw6hj1Z1cg",
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

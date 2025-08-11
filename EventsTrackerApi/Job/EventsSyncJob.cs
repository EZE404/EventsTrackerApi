using System.Threading.Channels;
using EventsTrackerApi.Service;

namespace EventsTrackerApi.Job;

public class EventsSyncJob : BackgroundService
{
    private readonly ILogger<EventsSyncJob> _logger;
    private readonly FcmService _fcm;

    public EventsSyncJob(ILogger<EventsSyncJob> logger, FcmService fcm)
    {
        _logger = logger;
        _fcm = fcm;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
         _logger.LogInformation("Corriendo cron de eventos...");
        var timer = new PeriodicTimer(TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30)));
        //var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

       // await RunCronJob(stoppingToken);

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

            await _fcm.SendToDivaceTokenAsync(
                deviceToken: "cfxtbBzfRZSaQohVaBehds:APA91bHq-taWjEnUsjVd10PkIELxJwhlsim3yOz3sGx5N5H987jBD5Omyqhd2XWcEs6WWmkLcTVCd-8oQpA95_qzYRTQ7Ik_QdfohKvu0PBTkPmC-_l3r54",
                title: "Sync de eventos",
                body: "Se han sincronizado las eventos, con cron"
            );

            _logger.LogInformation("Notificación enviada al topic 'events'.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando notificación al topic 'events'");
        }
    }
}

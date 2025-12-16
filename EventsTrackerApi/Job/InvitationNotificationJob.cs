using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using EventsTrackerApi.Repositories;
using EventsTrackerApi.Service;

public class InvitationNotificationJob(
    IServiceScopeFactory scopeFactory,
    FcmService fcm,
    ILogger<InvitationNotificationJob> logger
) : BackgroundService
{
    private const int MinutesToSync =15;
    private const int MaxDegreeOfParallelism = 12;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("InvitationJob: START. interval={Minutes}min maxDop={MaxDop}",
            MinutesToSync, MaxDegreeOfParallelism);

        var timer = new PeriodicTimer(TimeSpan.FromMinutes(MinutesToSync));

        // corre una vez al iniciar
        await Run(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            logger.LogDebug("InvitationJob: TICK nowUtc={Now:u}", DateTime.UtcNow);
            await Run(stoppingToken);
        }

        logger.LogInformation("InvitationJob: STOP (cancellation requested). nowUtc={Now:u}", DateTime.UtcNow);
    }

    private async Task Run(CancellationToken ct)
    {
        var runId = Guid.NewGuid().ToString("N")[..8];
        var sw = Stopwatch.StartNew();

        logger.LogInformation("InvitationJob[{RunId}]: RUN START nowUtc={Now:u}", runId, DateTime.UtcNow);

        try
        {
            using var scope = scopeFactory.CreateScope();

            var invitationRepo = scope.ServiceProvider.GetRequiredService<IEventInvitationRepository>();
            var deviceRepo = scope.ServiceProvider.GetRequiredService<IDevicesRepository>();

            // 1) Invitaciones pendientes no notificadas
            var invitations = await invitationRepo.GetPendingUnnotifiedAsync(ct);
            logger.LogInformation("InvitationJob[{RunId}]: pendingInvitations={Count}", runId, invitations.Count);

            if (invitations.Count == 0)
            {
                logger.LogInformation("InvitationJob[{RunId}]: RUN END (nothing to do) elapsedMs={Elapsed}",
                    runId, sw.ElapsedMilliseconds);
                return;
            }

            var distinctUsersFromInv = invitations.Select(i => i.UserId).Distinct().Count();
            logger.LogInformation("InvitationJob[{RunId}]: distinctUsersWithInvitations={Users}", runId, distinctUsersFromInv);

            // 2) Tokens activos
            logger.LogDebug("InvitationJob[{RunId}]: loading device tokens...", runId);

            var deviceTokens = await deviceRepo
                .FindAsync(d => d.IsActive) // si no existe IsActive, cambialo por d => true
                .Select(d => new { d.UserId, d.Token })
                .Where(x => !string.IsNullOrWhiteSpace(x.Token))
                .ToListAsync(ct);

            var uniqueTokensCount = deviceTokens.Select(x => x.Token).Distinct().Count();

            logger.LogInformation("InvitationJob[{RunId}]: deviceTokens={Tokens} uniqueTokens={Unique} usersWithTokens={Users}",
                runId, deviceTokens.Count, uniqueTokensCount, deviceTokens.Select(x => x.UserId).Distinct().Count());

            if (deviceTokens.Count == 0)
            {
                logger.LogWarning("InvitationJob[{RunId}]: no tokens -> cannot notify. elapsedMs={Elapsed}",
                    runId, sw.ElapsedMilliseconds);
                return;
            }

            // 3) Agrupar invitaciones por usuario
            var grouped = invitations
                .GroupBy(i => i.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Invitations = g.ToList()
                })
                .ToList();

            logger.LogInformation("InvitationJob[{RunId}]: workItems(users)={Count}", runId, grouped.Count);

            var tokensByUser = deviceTokens
                .GroupBy(x => x.UserId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Token).Distinct().ToList());

            var notifiedIds = new ConcurrentBag<int>();
            var errors = new ConcurrentBag<(int UserId, string Token, Exception Ex)>();
            var usersNotified = 0;

            await Parallel.ForEachAsync(grouped, new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                CancellationToken = ct
            },
            async (item, tok) =>
            {
                if (!tokensByUser.TryGetValue(item.UserId, out var tokens) || tokens.Count == 0)
                {
                    logger.LogDebug("InvitationJob[{RunId}]: skip user={UserId} (no tokens)",
                        runId, item.UserId);
                    return;
                }

                var count = item.Invitations.Count;

                var preview = string.Join(
                    " • ",
                    item.Invitations
                        .Select(i => i.Event?.Name)
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .Distinct()
                        .Take(4)
                );

                var title = count == 1
                    ? "Tenés una invitación a un evento"
                    : $"Tenés {count} invitaciones nuevas";

                var body = string.IsNullOrWhiteSpace(preview)
                    ? "Tocá para ver tus invitaciones"
                    : preview;

                var eventIdsCsv = string.Join(",", item.Invitations.Select(i => i.EventId).Distinct());

                var firstInvitationId = item.Invitations[0].Id;

                var data = new Dictionary<string, string>
                {
                    ["destination"] = (count == 1) ? "invitation_detail" : "invitations",
                    ["count"] = count.ToString(),
                    ["invitationId"] = firstInvitationId.ToString(), // ✅ clave
                    ["eventIds"] = eventIdsCsv,
                    ["preview"] = preview,
                    ["title"] = title,
                    ["body"] = body
                };

                logger.LogInformation(
                    "InvitationJob[{RunId}]: notify user={UserId} invitations={InvCount} tokens={TokCount} eventIds={EventIds} preview='{Preview}'",
                    runId, item.UserId, count, tokens.Count, eventIdsCsv, preview
                );

                var ok = 0;
                foreach (var token in tokens)
                {
                    try
                    {
                        await fcm.SendToTokenAsync(token, title, body, data, ct: tok);
                        ok++;

                        logger.LogDebug(
                            "InvitationJob[{RunId}]: FCM OK user={UserId} tokenPrefix={TokenPrefix}",
                            runId, item.UserId, token[..Math.Min(12, token.Length)]
                        );
                    }
                    catch (Exception ex)
                    {
                        errors.Add((item.UserId, token, ex));
                    }
                }

                if (ok > 0)
                {
                    Interlocked.Increment(ref usersNotified);

                    foreach (var id in item.Invitations.Select(i => i.Id))
                        notifiedIds.Add(id);
                }
                else
                {
                    logger.LogWarning("InvitationJob[{RunId}]: user={UserId} no successful sends (all failed)",
                        runId, item.UserId);
                }
            });

            // Errores individuales
            if (!errors.IsEmpty)
            {
                logger.LogWarning("InvitationJob[{RunId}]: sendErrors={Count}", runId, errors.Count);

                foreach (var err in errors)
                {
                    logger.LogWarning(err.Ex,
                        "InvitationJob[{RunId}]: error sending user={UserId} tokenPrefix={TokenPrefix}",
                        runId, err.UserId, err.Token[..Math.Min(12, err.Token.Length)]);
                }
            }

            // 4) Marcar como notificadas (batch)
            /* var ids = notifiedIds.Distinct().ToList();
             logger.LogInformation("InvitationJob[{RunId}]: willMarkNotified invitations={Count} usersNotified={UsersNotified}",
                 runId, ids.Count, usersNotified);

             if (ids.Count > 0)
             {
                 await invitationRepo.MarkAsNotifiedAsync(ids, ct);
                 logger.LogInformation("InvitationJob[{RunId}]: marked NotifiedAt OK invitations={Count}",
                     runId, ids.Count);
             }
             else
             {
                 logger.LogInformation("InvitationJob[{RunId}]: nothing marked (no successful sends).", runId);
             }*/

            logger.LogInformation("InvitationJob[{RunId}]: RUN END OK elapsedMs={Elapsed}",
                runId, sw.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            logger.LogInformation("InvitationJob[{RunId}]: RUN CANCELLED elapsedMs={Elapsed}",
                runId, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "InvitationJob[{RunId}]: RUN FAILED elapsedMs={Elapsed}",
                runId, sw.ElapsedMilliseconds);
        }
        finally
        {
            sw.Stop();
        }
    }
}

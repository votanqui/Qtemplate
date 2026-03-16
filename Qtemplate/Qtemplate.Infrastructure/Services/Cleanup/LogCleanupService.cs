using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qtemplate.Application.Constants;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Services.Cleanup;

public class LogCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<LogCleanupService> _logger;

    public LogCleanupService(IServiceProvider services, ILogger<LogCleanupService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LogCleanupService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delay = ComputeDelayUntil(hour: 1, minute: 0); // 01:00 UTC
                _logger.LogInformation(
                    "LogCleanupService: next run in {Minutes:F0} minutes.", delay.TotalMinutes);

                await Task.Delay(delay, stoppingToken);
                await ProcessAsync();
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LogCleanupService error.");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        _logger.LogInformation("LogCleanupService stopped.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    private async Task ProcessAsync()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var settingRepo = scope.ServiceProvider.GetRequiredService<ISettingRepository>();
        var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();

        int retentionDays = await settingRepo.GetIntAsync(
            SettingKeys.LogRetentionDays, defaultValue: 90);

        var cutoff = DateTime.UtcNow.AddDays(-retentionDays);

        // ── AuditLogs ─────────────────────────────────────────────────────────
        int auditDeleted = await db.AuditLogs
            .Where(a => a.CreatedAt < cutoff)
            .ExecuteDeleteAsync();

        // ── RequestLogs ───────────────────────────────────────────────────────
        int requestDeleted = await db.RequestLogs
            .Where(r => r.CreatedAt < cutoff)
            .ExecuteDeleteAsync();

        if (auditDeleted > 0 || requestDeleted > 0)
        {
            _logger.LogInformation(
                "LogCleanupService: deleted {Audit} audit logs + {Request} request logs (retention={Days}d, cutoff={Cutoff:yyyy-MM-dd}).",
                auditDeleted, requestDeleted, retentionDays, cutoff);

            await auditLogService.LogAsync(
                userId: "SYSTEM",
                userEmail: "log-cleanup@system",
                action: "LogCleanup",
                entityName: "AuditLog+RequestLog",
                entityId: "BATCH",
                newValues: new { auditDeleted, requestDeleted, retentionDays, cutoff });
        }
        else
        {
            _logger.LogDebug("LogCleanupService: nothing to delete.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    private static TimeSpan ComputeDelayUntil(int hour, int minute)
    {
        var now = DateTime.UtcNow;
        var target = now.Date.AddHours(hour).AddMinutes(minute);
        if (target <= now) target = target.AddDays(1);
        return target - now;
    }
}
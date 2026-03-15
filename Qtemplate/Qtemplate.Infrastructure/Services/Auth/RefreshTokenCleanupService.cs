using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qtemplate.Application.Constants;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Services.Auth;


public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<RefreshTokenCleanupService> _logger;

    public RefreshTokenCleanupService(
        IServiceProvider services,
        ILogger<RefreshTokenCleanupService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RefreshTokenCleanupService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delay = ComputeDelayUntil(hour: 2, minute: 0); // 02:00 UTC
                _logger.LogInformation(
                    "RefreshTokenCleanupService: next run in {Minutes:F0} minutes.", delay.TotalMinutes);

                await Task.Delay(delay, stoppingToken);
                await ProcessAsync();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshTokenCleanupService error.");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        _logger.LogInformation("RefreshTokenCleanupService stopped.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    private static TimeSpan ComputeDelayUntil(int hour, int minute)
    {
        var now = DateTime.UtcNow;
        var target = now.Date.AddHours(hour).AddMinutes(minute);
        if (target <= now) target = target.AddDays(1); // nếu đã qua giờ hôm nay → hẹn ngày mai
        return target - now;
    }

    // ─────────────────────────────────────────────────────────────────────────
    private async Task ProcessAsync()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var settingRepo = scope.ServiceProvider.GetRequiredService<ISettingRepository>();

        int retentionDays = await settingRepo.GetIntAsync(
            SettingKeys.RefreshTokenRetentionDays, defaultValue: 30);

        var cutoff = DateTime.UtcNow.AddDays(-retentionDays);

        // Xoá bằng ExecuteDeleteAsync — không load entity vào RAM
        int deleted = await db.RefreshTokens
            .Where(t =>
                (t.IsRevoked && t.RevokedAt != null && t.RevokedAt < cutoff) ||
                (!t.IsRevoked && t.ExpiresAt < cutoff))
            .ExecuteDeleteAsync();

        if (deleted > 0)
            _logger.LogInformation(
                "RefreshTokenCleanupService: deleted {Count} stale tokens (retention={Days}d, cutoff={Cutoff:yyyy-MM-dd}).",
                deleted, retentionDays, cutoff);
        else
            _logger.LogDebug("RefreshTokenCleanupService: nothing to delete.");
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qtemplate.Application.Constants;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Services.Cleanup;

public class AnalyticsCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<AnalyticsCleanupService> _logger;

    public AnalyticsCleanupService(IServiceProvider services, ILogger<AnalyticsCleanupService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AnalyticsCleanupService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 00:05 UTC — 5 phút sau DailyStatAggregationService (00:00 UTC)
                // đảm bảo aggregate đã hoàn tất trước khi xóa raw data
                var delay = ComputeDelayUntil(hour: 0, minute: 5);
                _logger.LogInformation(
                    "AnalyticsCleanupService: next run in {Minutes:F0} minutes.", delay.TotalMinutes);

                await Task.Delay(delay, stoppingToken);
                await ProcessAsync();
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AnalyticsCleanupService error.");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        _logger.LogInformation("AnalyticsCleanupService stopped.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    private async Task ProcessAsync()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var settingRepo = scope.ServiceProvider.GetRequiredService<ISettingRepository>();

        int retentionDays = await settingRepo.GetIntAsync(
            SettingKeys.AnalyticsRetentionDays, defaultValue: 30);

        // Chỉ xóa data đủ cũ để DailyStatAggregationService đã xử lý hết
        // Safety: không bao giờ xóa trong 7 ngày gần nhất dù retention_days được set thấp hơn
        int effectiveDays = Math.Max(retentionDays, 7);
        var cutoff = DateTime.UtcNow.AddDays(-effectiveDays);

        int deleted = await db.Analytics
            .Where(a => a.CreatedAt < cutoff)
            .ExecuteDeleteAsync();

        if (deleted > 0)
            _logger.LogInformation(
                "AnalyticsCleanupService: deleted {Count} analytics events (retention={Days}d, cutoff={Cutoff:yyyy-MM-dd}).",
                deleted, effectiveDays, cutoff);
        else
            _logger.LogDebug("AnalyticsCleanupService: nothing to delete.");
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
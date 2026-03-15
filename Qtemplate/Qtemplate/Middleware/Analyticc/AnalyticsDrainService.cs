using Qtemplate.Domain.Entities;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Middleware.Analyticc;

public sealed class AnalyticsDrainService : BackgroundService
{
    private readonly AnalyticsQueue _queue;
    private readonly IServiceProvider _services;
    private readonly ILogger<AnalyticsDrainService> _logger;

    private const int BatchSize = 500;
    private static readonly TimeSpan DrainInterval = TimeSpan.FromSeconds(5);

    public AnalyticsDrainService(
        AnalyticsQueue queue,
        IServiceProvider services,
        ILogger<AnalyticsDrainService> logger)
    {
        _queue = queue;
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(DrainInterval, stoppingToken);
            await DrainAsync(stoppingToken);
        }

        // Flush lần cuối khi app shutdown — không bỏ mất data
        await DrainAsync(CancellationToken.None);
    }

    private async Task DrainAsync(CancellationToken ct)
    {
        var batch = new List<Analytics>(BatchSize);

        while (batch.Count < BatchSize && _queue.Reader.TryRead(out var item))
            batch.Add(item);

        if (batch.Count == 0) return;

        try
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Analytics.AddRangeAsync(batch, ct);
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi drain Analytics batch ({Count} items)", batch.Count);
        }
    }
}
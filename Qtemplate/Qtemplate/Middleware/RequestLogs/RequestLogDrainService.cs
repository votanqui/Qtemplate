using Qtemplate.Domain.Entities;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Middleware.RequestLogs;

public sealed class RequestLogDrainService : BackgroundService
{
    private readonly RequestLogQueue _queue;
    private readonly IServiceProvider _services;
    private readonly ILogger<RequestLogDrainService> _logger;

    private const int BatchSize = 200;
    private static readonly TimeSpan DrainInterval = TimeSpan.FromSeconds(2);

    public RequestLogDrainService(
        RequestLogQueue queue,
        IServiceProvider services,
        ILogger<RequestLogDrainService> logger)
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

        // Flush lần cuối khi app shutdown
        await DrainAsync(CancellationToken.None);
    }

    private async Task DrainAsync(CancellationToken ct)
    {
        var batch = new List<RequestLog>(BatchSize);

        while (batch.Count < BatchSize && _queue.Reader.TryRead(out var log))
            batch.Add(log);

        if (batch.Count == 0) return;

        try
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.RequestLogs.AddRangeAsync(batch, ct);
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi drain RequestLog batch ({Count} items)", batch.Count);
        }
    }
}
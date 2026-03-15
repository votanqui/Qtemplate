using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qtemplate.Infrastructure.Data;
using AuditLogEntity = Qtemplate.Domain.Entities.AuditLog;

namespace Qtemplate.Infrastructure.Services.AuditLog
{
    public sealed class AuditLogDrainService : BackgroundService
    {
        private readonly AuditLogQueue _queue;
        private readonly IServiceProvider _services;
        private readonly ILogger<AuditLogDrainService> _logger;

        private const int BatchSize = 100;
        private static readonly TimeSpan DrainInterval = TimeSpan.FromSeconds(3);

        public AuditLogDrainService(
            AuditLogQueue queue,
            IServiceProvider services,
            ILogger<AuditLogDrainService> logger)
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
            var batch = new List<AuditLogEntity>(BatchSize);

            while (batch.Count < BatchSize && _queue.Reader.TryRead(out var entry))
                batch.Add(entry);

            if (batch.Count == 0) return;

            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.AuditLogs.AddRangeAsync(batch, ct);
                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi drain AuditLog batch ({Count} items)", batch.Count);
            }
        }
    }
}

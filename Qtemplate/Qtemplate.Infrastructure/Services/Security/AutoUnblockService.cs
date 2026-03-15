using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Infrastructure.Services.Security
{
    public class AutoUnblockService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<AutoUnblockService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

        public AutoUnblockService(IServiceProvider services, ILogger<AutoUnblockService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AutoUnblockService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessAsync();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AutoUnblockService error.");
                }

                await Task.Delay(Interval, stoppingToken);
            }

            _logger.LogInformation("AutoUnblockService stopped.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        private async Task ProcessAsync()
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
            var now = DateTime.UtcNow;

            // ── 1. Unblock IP hết hạn ─────────────────────────────────────────────
            var expiredIps = await db.IpBlacklists
                .Where(x => x.IsActive && x.ExpiredAt.HasValue && x.ExpiredAt <= now)
                .ToListAsync();

            if (expiredIps.Count > 0)
            {
                foreach (var ip in expiredIps)
                {
                    ip.IsActive = false;

                    await auditLogService.LogAsync(
                        userId: "SYSTEM",
                        userEmail: "auto-unblock@system",
                        action: "AutoUnblockIp",
                        entityName: "IpBlacklist",
                        entityId: ip.Id.ToString(),
                        newValues: new { ip.IpAddress, UnblockedAt = now, Reason = "BlockExpired" });
                }

                await db.SaveChangesAsync();
                _logger.LogInformation("AutoUnblockService: unblocked {Count} expired IPs.", expiredIps.Count);
            }

            // ── 2. Unblock User hết hạn ───────────────────────────────────────────
            // Chỉ unblock user có BlockedUntil (block tạm từ scanner).
            // User bị admin tay khoá (BlockedUntil = null) sẽ không bị tự mở.
            var expiredUsers = await db.Users
                .Where(u => !u.IsActive && u.BlockedUntil.HasValue && u.BlockedUntil <= now)
                .ToListAsync();

            if (expiredUsers.Count > 0)
            {
                foreach (var user in expiredUsers)
                {
                    user.IsActive = true;
                    user.BlockedUntil = null;
                    user.UpdatedAt = now;

                    await auditLogService.LogAsync(
                        userId: "SYSTEM",
                        userEmail: "auto-unblock@system",
                        action: "AutoUnblockUser",
                        entityName: "User",
                        entityId: user.Id.ToString(),
                        newValues: new { user.Email, UnblockedAt = now, Reason = "BlockExpired" });
                }

                await db.SaveChangesAsync();
                _logger.LogInformation("AutoUnblockService: unblocked {Count} expired users.", expiredUsers.Count);
            }
        }
    }
}

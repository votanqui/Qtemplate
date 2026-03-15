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

namespace Qtemplate.Infrastructure.Services.Coupon
{
    public class CouponAutoDisableService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<CouponAutoDisableService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);

        public CouponAutoDisableService(IServiceProvider services, ILogger<CouponAutoDisableService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CouponAutoDisableService started.");

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
                    _logger.LogError(ex, "CouponAutoDisableService error.");
                }

                await Task.Delay(Interval, stoppingToken);
            }

            _logger.LogInformation("CouponAutoDisableService stopped.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        private async Task ProcessAsync()
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
            var now = DateTime.UtcNow;

            // Tìm tất cả coupon active cần disable:
            //   1. ExpiredAt đã qua
            //   2. Đã dùng hết UsageLimit
            var toDisable = await db.Coupons
                .Where(c => c.IsActive && (
                    (c.ExpiredAt.HasValue && c.ExpiredAt <= now) ||
                    (c.UsageLimit.HasValue && c.UsedCount >= c.UsageLimit)
                ))
                .ToListAsync();

            if (toDisable.Count == 0) return;

            foreach (var coupon in toDisable)
            {
                var reason = (coupon.ExpiredAt.HasValue && coupon.ExpiredAt <= now)
                    ? "Expired"
                    : "UsageLimitReached";

                coupon.IsActive = false;

                await auditLogService.LogAsync(
                    userId: "SYSTEM",
                    userEmail: "coupon-scheduler@system",
                    action: "AutoDisableCoupon",
                    entityName: "Coupon",
                    entityId: coupon.Id.ToString(),
                    newValues: new
                    {
                        coupon.Code,
                        Reason = reason,
                        DisabledAt = now,
                        coupon.ExpiredAt,
                        coupon.UsedCount,
                        coupon.UsageLimit
                    });
            }

            await db.SaveChangesAsync();
            _logger.LogInformation(
                "CouponAutoDisableService: disabled {Count} coupons.", toDisable.Count);
        }
    }
}

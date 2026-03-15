using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qtemplate.Application.Constants;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Infrastructure.Services.Affiliate
{
    public class AffiliateCommissionService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<AffiliateCommissionService> _logger;

        public AffiliateCommissionService(
            IServiceProvider services,
            ILogger<AffiliateCommissionService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AffiliateCommissionService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var delay = ComputeDelayUntil(hour: 3, minute: 0); // 03:00 UTC
                    _logger.LogInformation(
                        "AffiliateCommissionService: next run in {Minutes:F0} minutes.", delay.TotalMinutes);

                    await Task.Delay(delay, stoppingToken);
                    await ProcessAsync();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AffiliateCommissionService error.");
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }

            _logger.LogInformation("AffiliateCommissionService stopped.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        private static TimeSpan ComputeDelayUntil(int hour, int minute)
        {
            var now = DateTime.UtcNow;
            var target = now.Date.AddHours(hour).AddMinutes(minute);
            if (target <= now) target = target.AddDays(1);
            return target - now;
        }

        // ─────────────────────────────────────────────────────────────────────────
        private async Task ProcessAsync()
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var settingRepo = scope.ServiceProvider.GetRequiredService<ISettingRepository>();
            var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
            var notiService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            // ── Đọc setting ───────────────────────────────────────────────────────
            int autoApproveDays = await settingRepo.GetIntAsync(
                SettingKeys.AffiliateAutoApproveDays, defaultValue: 7);

            if (autoApproveDays <= 0)
            {
                _logger.LogDebug("AffiliateCommissionService: disabled (affiliate.auto_approve_days=0).");
                return;
            }

            var cutoff = DateTime.UtcNow.AddDays(-autoApproveDays);

            // ── Lấy tất cả Pending transaction đã đủ ngày ────────────────────────
            var pendingTxs = await db.AffiliateTransactions
                .Include(t => t.Affiliate)
                    .ThenInclude(a => a.User)
                .Where(t => t.Status == "Pending" && t.CreatedAt <= cutoff)
                .ToListAsync();

            if (pendingTxs.Count == 0)
            {
                _logger.LogDebug("AffiliateCommissionService: no transactions to approve.");
                return;
            }

            // Group theo Affiliate để cập nhật PendingAmount 1 lần mỗi affiliate
            var byAffiliate = pendingTxs.GroupBy(t => t.AffiliateId);

            foreach (var group in byAffiliate)
            {
                var affiliate = group.First().Affiliate;
                decimal totalComm = group.Sum(t => t.Commission);

                // ── Approve từng transaction ──────────────────────────────────────
                foreach (var tx in group)
                {
                    tx.Status = "Approved";

                    await auditLogService.LogAsync(
                        userId: "SYSTEM",
                        userEmail: "affiliate-scheduler@system",
                        action: "AutoApproveCommission",
                        entityName: "AffiliateTransaction",
                        entityId: tx.Id.ToString(),
                        newValues: new
                        {
                            tx.Commission,
                            tx.OrderId,
                            ApprovedAt = DateTime.UtcNow,
                            AutoApproveDays = autoApproveDays
                        });
                }

                // ── Trừ PendingAmount trên Affiliate ─────────────────────────────
                affiliate.PendingAmount = Math.Max(0, affiliate.PendingAmount - totalComm);
                // Không cộng thêm TotalEarned — đã cộng lúc tạo transaction trong SepayCallbackHandler

                // ── Gửi notification cho affiliate owner ─────────────────────────
                await notiService.SendToUserAsync(
                    userId: affiliate.UserId,
                    title: "Hoa hồng đã được duyệt",
                    message: $"{group.Count()} giao dịch hoa hồng ({totalComm:N0}₫) đã được tự động duyệt và sẵn sàng để rút.",
                    type: "Success",
                    redirectUrl: "/dashboard/affiliate");
            }

            await db.SaveChangesAsync();

            _logger.LogInformation(
                "AffiliateCommissionService: approved {TxCount} transactions across {AffCount} affiliates.",
                pendingTxs.Count, byAffiliate.Count());
        }
    }
}

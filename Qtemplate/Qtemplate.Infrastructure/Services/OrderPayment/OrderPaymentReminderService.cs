using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qtemplate.Application.Constants;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Application.Services;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Infrastructure.Services.OrderPayment
{
    public class OrderPaymentReminderService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<OrderPaymentReminderService> _logger;

        public OrderPaymentReminderService(
            IServiceProvider services,
            ILogger<OrderPaymentReminderService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderPaymentReminderService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OrderPaymentReminderService error. Retry in 1 min.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("OrderPaymentReminderService stopped.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        private async Task ProcessAsync(CancellationToken ct)
        {
            using var scope = _services.CreateScope();
            var sp = scope.ServiceProvider;

            var settingRepo = sp.GetRequiredService<ISettingRepository>();
            var orderRepo = sp.GetRequiredService<IOrderRepository>();
            var emailService = sp.GetRequiredService<IEmailService>();
            var notiService = sp.GetRequiredService<INotificationService>();
            var auditLog = sp.GetRequiredService<IAuditLogService>();

            // ── 1. Đọc settings ──────────────────────────────────────────────────
            var enabledRaw = await settingRepo.GetValueAsync(SettingKeys.OrderReminderEnabled);
            if (string.Equals(enabledRaw, "false", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("OrderPaymentReminderService: disabled via setting.");
                return;
            }

            int reminderMinutes = await settingRepo.GetIntAsync(
                SettingKeys.OrderPaymentReminderMinutes, defaultValue: 30);
            int cancelMinutes = await settingRepo.GetIntAsync(
                SettingKeys.OrderAutoCancelMinutes, defaultValue: 60);
            var siteUrl = await settingRepo.GetValueAsync(SettingKeys.SiteUrl)
                          ?? "https://qtemplate.vn";

            // ── 2. Gửi nhắc nhở ──────────────────────────────────────────────────
            var toRemind = await orderRepo.GetPendingForReminderAsync(reminderMinutes, cancelMinutes);

            foreach (var order in toRemind)
            {
                if (ct.IsCancellationRequested) break;
                try
                {
                    var user = order.User;
                    var minutesLeft = cancelMinutes - reminderMinutes;
                    var paymentUrl = $"{siteUrl}/dashboard/orders/{order.Id}";

                    // Email nhắc nhở
                    await emailService.SendAsync(
                        toEmail: user.Email,
                        subject: $"⏰ Nhắc nhở thanh toán đơn hàng {order.OrderCode}",
                        htmlBody: EmailTemplates.PaymentReminder(
                                      user.FullName, order.OrderCode,
                                      order.FinalAmount, minutesLeft, paymentUrl),
                        template: "payment_reminder");

                    // In-app notification
                    await notiService.SendToUserAsync(
                        userId: user.Id,
                        title: "⏰ Nhắc nhở thanh toán",
                        message: $"Đơn hàng {order.OrderCode} ({order.FinalAmount:N0}đ) sẽ hết hạn sau {minutesLeft} phút.",
                        type: "Warning",
                        redirectUrl: $"/dashboard/orders/{order.Id}");

                    // Đánh dấu đã nhắc — tránh gửi lại
                    order.ReminderSentAt = DateTime.UtcNow;
                    await orderRepo.UpdateAsync(order);

                    _logger.LogInformation("Reminder sent → order {Code}", order.OrderCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send reminder for order {Code}", order.OrderCode);
                }
            }

            // ── 3. Auto-cancel ────────────────────────────────────────────────────
            var toCancel = await orderRepo.GetPendingForAutoCancelAsync(cancelMinutes);

            foreach (var order in toCancel)
            {
                if (ct.IsCancellationRequested) break;
                try
                {
                    var user = order.User;

                    // Cập nhật trạng thái → Cancelled
                    order.Status = "Cancelled";
                    order.CancelReason = $"Tự động hủy: quá {cancelMinutes} phút chưa thanh toán.";
                    order.CancelledAt = DateTime.UtcNow;
                    order.UpdatedAt = DateTime.UtcNow;
                    await orderRepo.UpdateAsync(order);

                    // Email thông báo hủy
                    await emailService.SendAsync(
                        toEmail: user.Email,
                        subject: $"🚫 Đơn hàng {order.OrderCode} đã bị hủy tự động",
                        htmlBody: EmailTemplates.OrderAutoCancelled(
                                      user.FullName, order.OrderCode, order.FinalAmount),
                        template: "order_auto_cancelled");

                    // In-app notification
                    await notiService.SendToUserAsync(
                        userId: user.Id,
                        title: "🚫 Đơn hàng đã bị hủy",
                        message: $"Đơn hàng {order.OrderCode} đã bị hủy tự động do quá thời gian thanh toán.",
                        type: "Error",
                        redirectUrl: $"/dashboard/orders/{order.Id}");

                    // Audit log (actor = system)
                    await auditLog.LogAsync(
                        userId: null,
                        userEmail: "system@qtemplate",
                        action: "AutoCancelOrder",
                        entityName: "Order",
                        entityId: order.Id.ToString(),
                        newValues: new { order.Status, order.CancelReason, order.CancelledAt });

                    _logger.LogInformation("Auto-cancelled order {Code}", order.OrderCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to auto-cancel order {Code}", order.OrderCode);
                }
            }

            if (toRemind.Count > 0 || toCancel.Count > 0)
                _logger.LogInformation(
                    "OrderPaymentReminderService: reminded={R}, cancelled={C}",
                    toRemind.Count, toCancel.Count);
        }
    }
}

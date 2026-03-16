using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qtemplate.Application.Constants;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Services.Ticket;

public class TicketAutoCloseService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<TicketAutoCloseService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    public TicketAutoCloseService(IServiceProvider services, ILogger<TicketAutoCloseService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TicketAutoCloseService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAsync();
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TicketAutoCloseService error.");
            }

            await Task.Delay(Interval, stoppingToken);
        }

        _logger.LogInformation("TicketAutoCloseService stopped.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    private async Task ProcessAsync()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var settingRepo = scope.ServiceProvider.GetRequiredService<ISettingRepository>();
        var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
        var notifService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        // Kiểm tra feature có bật không
        var enabled = await settingRepo.GetValueAsync(SettingKeys.TicketAutoCloseEnabled);
        if (string.Equals(enabled, "false", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("TicketAutoCloseService: disabled via setting.");
            return;
        }

        int autoCloseDays = await settingRepo.GetIntAsync(
            SettingKeys.TicketAutoCloseDays, defaultValue: 7);

        var cutoff = DateTime.UtcNow.AddDays(-autoCloseDays);
        var now = DateTime.UtcNow;

        // Lấy ticket Open/InProgress không có reply mới sau cutoff
        // Logic: ticket.UpdatedAt < cutoff VÀ không có TicketReply nào sau cutoff
        var staleTickets = await db.SupportTickets
            .Where(t =>
                (t.Status == "Open" || t.Status == "InProgress") &&
                t.CreatedAt < cutoff &&
                !db.TicketReplies.Any(r =>
                    r.TicketId == t.Id && r.CreatedAt >= cutoff))
            .Include(t => t.User)
            .ToListAsync();

        if (staleTickets.Count == 0)
        {
            _logger.LogDebug("TicketAutoCloseService: no stale tickets found.");
            return;
        }

        foreach (var ticket in staleTickets)
        {
            var previousStatus = ticket.Status;

            ticket.Status = "Closed";
            ticket.ClosedAt = now;
            ticket.UpdatedAt = now;

            // Thêm reply hệ thống để người dùng biết lý do đóng
            await db.TicketReplies.AddAsync(new Domain.Entities.TicketReply
            {
                TicketId = ticket.Id,
                UserId = ticket.UserId,   // đặt UserId = owner ticket cho FK hợp lệ
                Message = $"Ticket này đã được tự động đóng do không có phản hồi trong {autoCloseDays} ngày. " +
                               $"Nếu vấn đề chưa được giải quyết, vui lòng tạo ticket mới.",
                IsFromAdmin = true,
                CreatedAt = now
            });

            await auditLogService.LogAsync(
                userId: "SYSTEM",
                userEmail: "ticket-autoclose@system",
                action: "TicketAutoClose",
                entityName: "SupportTicket",
                entityId: ticket.Id.ToString(),
                oldValues: new { Status = previousStatus },
                newValues: new { Status = "Closed", ClosedAt = now, Reason = $"NoActivityFor{autoCloseDays}Days" });

            // Gửi notification cho user
            _ = notifService.SendToUserAsync(
                userId: ticket.UserId,
                title: "Ticket hỗ trợ đã đóng tự động",
                message: $"Ticket #{ticket.TicketCode} — \"{ticket.Subject}\" đã được đóng tự động do không có phản hồi trong {autoCloseDays} ngày.",
                type: "Warning",
                redirectUrl: $"/tickets/{ticket.Id}");
        }

        await db.SaveChangesAsync();

        _logger.LogInformation(
            "TicketAutoCloseService: closed {Count} stale tickets (no activity > {Days}d).",
            staleTickets.Count, autoCloseDays);
    }
}
using Microsoft.AspNetCore.SignalR;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Hubs;
using NotificationEntity = Qtemplate.Domain.Entities.Notification;

namespace Qtemplate.Infrastructure.Services.Notification;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notifRepo;
    private readonly IUserRepository _userRepo;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(
        INotificationRepository notifRepo,
        IUserRepository userRepo,
        IHubContext<NotificationHub> hubContext)
    {
        _notifRepo = notifRepo;
        _userRepo = userRepo;
        _hubContext = hubContext;
    }

    // ── Gửi 1 user — lưu DB + socket ─────────────────────────────────────────
    public async Task SendToUserAsync(Guid userId, string title, string message,
        string type = "Info", string? redirectUrl = null)
    {
        var notification = new NotificationEntity
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            RedirectUrl = redirectUrl
        };
        await _notifRepo.AddAsync(notification);

        await _hubContext.Clients
            .Group($"user_{userId}")
            .SendAsync("ReceiveNotification", new
            {
                notification.Id,
                notification.Title,
                notification.Message,
                notification.Type,
                notification.RedirectUrl,
                notification.IsRead,
                notification.CreatedAt
            });
    }

    // ── Broadcast socket only — KHÔNG lưu DB (backward compatible) ────────────
    public async Task BroadcastAsync(string title, string message,
        string type = "Info", string? redirectUrl = null)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
        {
            Id = 0,
            Title = title,
            Message = message,
            Type = type,
            RedirectUrl = redirectUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
    }

    // ── Broadcast toàn bộ user — lưu DB cho từng user + socket ───────────────
    public async Task BroadcastToAllAsync(string title, string message,
        string type = "Info", string? redirectUrl = null)
    {
        // Lấy tất cả userId đang active
        var userIds = await _userRepo.GetAllActiveUserIdsAsync();
        if (userIds.Count == 0) return;

        var now = DateTime.UtcNow;

        // Lưu DB hàng loạt (1 lần SaveChanges)
        var notifications = userIds.Select(uid => new NotificationEntity
        {
            UserId = uid,
            Title = title,
            Message = message,
            Type = type,
            RedirectUrl = redirectUrl,
            CreatedAt = now
        }).ToList();

        await _notifRepo.AddRangeAsync(notifications);

        // Gửi socket — broadcast all (user nào đang online sẽ nhận ngay)
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
        {
            Id = 0,
            Title = title,
            Message = message,
            Type = type,
            RedirectUrl = redirectUrl,
            IsRead = false,
            CreatedAt = now
        });
    }

    // ── Gửi nhiều user cụ thể — lưu DB + socket ──────────────────────────────
    public async Task SendToUsersAsync(IEnumerable<Guid> userIds, string title, string message,
        string type = "Info", string? redirectUrl = null)
    {
        var userIdList = userIds.ToList();
        var now = DateTime.UtcNow;

        var notifications = userIdList.Select(uid => new NotificationEntity
        {
            UserId = uid,
            Title = title,
            Message = message,
            Type = type,
            RedirectUrl = redirectUrl,
            CreatedAt = now
        }).ToList();

        await _notifRepo.AddRangeAsync(notifications);

        foreach (var uid in userIdList)
        {
            await _hubContext.Clients
                .Group($"user_{uid}")
                .SendAsync("ReceiveNotification", new
                {
                    Title = title,
                    Message = message,
                    Type = type,
                    RedirectUrl = redirectUrl,
                    IsRead = false,
                    CreatedAt = now
                });
        }
    }
}
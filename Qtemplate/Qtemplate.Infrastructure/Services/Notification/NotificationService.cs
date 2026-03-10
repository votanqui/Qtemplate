using Microsoft.AspNetCore.SignalR;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Hubs;
using NotificationEntity = Qtemplate.Domain.Entities.Notification; // ← thêm dòng này

namespace Qtemplate.Infrastructure.Services.Notification;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notifRepo;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(
        INotificationRepository notifRepo,
        IHubContext<NotificationHub> hubContext)
    {
        _notifRepo = notifRepo;
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(Guid userId, string title, string message,
        string type = "Info", string? redirectUrl = null)
    {
        var notification = new NotificationEntity  // ← dùng alias
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

    public async Task SendToUsersAsync(IEnumerable<Guid> userIds, string title, string message,
        string type = "Info", string? redirectUrl = null)
    {
        var userIdList = userIds.ToList();
        var now = DateTime.UtcNow;

        var notifications = userIdList.Select(uid => new NotificationEntity  // ← dùng alias
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
namespace Qtemplate.Application.Services.Interfaces;

public interface INotificationService
{
    // Gửi đến 1 user cụ thể — lưu DB + socket
    Task SendToUserAsync(Guid userId, string title, string message,
                         string type = "Info", string? redirectUrl = null);

    // Broadcast socket only (không lưu DB) — giữ để backward compatible
    Task BroadcastAsync(string title, string message,
                        string type = "Info", string? redirectUrl = null);

    // Broadcast toàn bộ user — lưu DB cho từng user + socket
    Task BroadcastToAllAsync(string title, string message,
                              string type = "Info", string? redirectUrl = null);

    // Gửi đến nhiều user (theo danh sách userId) — lưu DB + socket
    Task SendToUsersAsync(IEnumerable<Guid> userIds, string title, string message,
                          string type = "Info", string? redirectUrl = null);
}
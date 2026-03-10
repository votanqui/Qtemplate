using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Admin.Notifications.Commands.SendNotification;

public class SendNotificationCommand : IRequest<ApiResponse<bool>>
{
    public Guid? UserId { get; set; }          // null = broadcast tất cả
    public List<Guid>? UserIds { get; set; }    // gửi nhiều user
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info"; // Info / Success / Warning
    public string? RedirectUrl { get; set; }
}
using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;

namespace Qtemplate.Application.Features.Admin.Notifications.Commands.SendNotification;

public class SendNotificationHandler : IRequestHandler<SendNotificationCommand, ApiResponse<bool>>
{
    private readonly INotificationService _notifService;

    public SendNotificationHandler(INotificationService notifService)
        => _notifService = notifService;

    public async Task<ApiResponse<bool>> Handle(
        SendNotificationCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId.HasValue)
        {
            // Gửi 1 user
            await _notifService.SendToUserAsync(
                request.UserId.Value, request.Title, request.Message,
                request.Type, request.RedirectUrl);
        }
        else if (request.UserIds is { Count: > 0 })
        {
            // Gửi nhiều user
            await _notifService.SendToUsersAsync(
                request.UserIds, request.Title, request.Message,
                request.Type, request.RedirectUrl);
        }
        else
        {
            // Broadcast tất cả
            await _notifService.BroadcastAsync(
                request.Title, request.Message,
                request.Type, request.RedirectUrl);
        }

        return ApiResponse<bool>.Ok(true, "Đã gửi thông báo thành công");
    }
}
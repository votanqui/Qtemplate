using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.UserManagement.Commands.MarkNotificationRead
{
    public class MarkNotificationReadHandler : IRequestHandler<MarkNotificationReadCommand, ApiResponse<bool>>
    {
        private readonly INotificationRepository _notifRepo;

        public MarkNotificationReadHandler(INotificationRepository notifRepo)
            => _notifRepo = notifRepo;

        public async Task<ApiResponse<bool>> Handle(
            MarkNotificationReadCommand request, CancellationToken cancellationToken)
        {
            if (request.NotificationId.HasValue)
            {
                var notif = await _notifRepo.GetByIdAsync(request.NotificationId.Value, request.UserId);
                if (notif is null)
                    return ApiResponse<bool>.Fail("Không tìm thấy thông báo");

                notif.IsRead = true;
                await _notifRepo.UpdateAsync(notif);
            }
            else
            {
                var unread = await _notifRepo.GetUnreadByUserIdAsync(request.UserId);
                foreach (var n in unread) n.IsRead = true;
                await _notifRepo.UpdateRangeAsync(unread);
            }

            return ApiResponse<bool>.Ok(true, "Đã đánh dấu đã đọc");
        }
    }
}

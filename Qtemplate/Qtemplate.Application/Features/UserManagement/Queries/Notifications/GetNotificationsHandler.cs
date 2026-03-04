using MediatR;
using Qtemplate.Application.DTOs.User;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.UserManagement.Queries.Notifications
{
    public class GetNotificationsHandler
    : IRequestHandler<GetNotificationsQuery, ApiResponse<PagedResultDto<NotificationDto>>>
    {
        private readonly INotificationRepository _notifRepo;

        public GetNotificationsHandler(INotificationRepository notifRepo)
            => _notifRepo = notifRepo;

        public async Task<ApiResponse<PagedResultDto<NotificationDto>>> Handle(
            GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            var (notifications, total) = await _notifRepo.GetPagedByUserIdAsync(
                request.UserId, request.Page, request.PageSize, request.UnreadOnly);

            var items = notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                RedirectUrl = n.RedirectUrl,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();

            return ApiResponse<PagedResultDto<NotificationDto>>.Ok(new PagedResultDto<NotificationDto>
            {
                Items = items,
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
    }
}

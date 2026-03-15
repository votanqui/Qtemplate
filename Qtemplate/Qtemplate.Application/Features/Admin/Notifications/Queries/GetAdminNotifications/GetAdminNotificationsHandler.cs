using MediatR;
using Qtemplate.Application.DTOs.Notification;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Admin.Notifications.Queries.GetAdminNotifications
{
    public class GetAdminNotificationsHandler
      : IRequestHandler<GetAdminNotificationsQuery, ApiResponse<PaginatedResult<AdminNotificationDto>>>
    {
        private readonly INotificationRepository _notifRepo;
        public GetAdminNotificationsHandler(INotificationRepository notifRepo) => _notifRepo = notifRepo;

        public async Task<ApiResponse<PaginatedResult<AdminNotificationDto>>> Handle(
            GetAdminNotificationsQuery request, CancellationToken cancellationToken)
        {
            var (notifications, total) = await _notifRepo.GetAdminPagedAsync(
                request.UserId, request.Type, request.Search, request.Page, request.PageSize);

            var items = notifications.Select(n => new AdminNotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                UserEmail = n.User.Email,
                UserName = n.User.FullName,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                RedirectUrl = n.RedirectUrl,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
            }).ToList();

            return ApiResponse<PaginatedResult<AdminNotificationDto>>.Ok(new PaginatedResult<AdminNotificationDto>
            {
                Items = items,
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize,
            });
        }
    }
}

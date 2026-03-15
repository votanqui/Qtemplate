using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Admin.Notifications.Queries.GetAdminNotifications
{
    public class GetAdminNotificationsQuery : IRequest<ApiResponse<PaginatedResult<AdminNotificationDto>>>
    {
        public Guid? UserId { get; set; }
        public string? Type { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

}

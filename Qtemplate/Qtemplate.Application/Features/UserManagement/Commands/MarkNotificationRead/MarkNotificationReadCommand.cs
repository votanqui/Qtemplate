using MediatR;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.UserManagement.Commands.MarkNotificationRead
{
    public class MarkNotificationReadCommand : IRequest<ApiResponse<bool>>
    {
        public Guid UserId { get; set; }
        public int? NotificationId { get; set; } // null = mark all
    }
}

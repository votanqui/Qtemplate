using MediatR;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Orders.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommand : IRequest<ApiResponse<object>>
    {
        public Guid OrderId { get; set; }
        public Guid AdminId { get; set; }
        public string AdminEmail { get; set; } = string.Empty;
        /// <summary>Paid | Completed</summary>
        public string NewStatus { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}

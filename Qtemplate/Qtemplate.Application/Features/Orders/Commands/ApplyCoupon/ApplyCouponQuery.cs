using MediatR;
using Qtemplate.Application.DTOs.Order;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Orders.Commands.ApplyCoupon
{
    public class ApplyCouponQuery : IRequest<ApiResponse<ApplyCouponResultDto>>
    {
        public string CouponCode { get; set; } = string.Empty;
        public List<Guid> TemplateIds { get; set; } = new();
    }
}

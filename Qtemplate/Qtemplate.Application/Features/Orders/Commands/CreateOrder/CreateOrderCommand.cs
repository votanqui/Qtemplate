using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Order;

namespace Qtemplate.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommand : IRequest<ApiResponse<OrderDto>>
{
    public Guid UserId { get; set; }
    public List<Guid> TemplateIds { get; set; } = new();
    public string? CouponCode { get; set; }
    public string? AffiliateCode { get; set; }
    public string? Note { get; set; }
}
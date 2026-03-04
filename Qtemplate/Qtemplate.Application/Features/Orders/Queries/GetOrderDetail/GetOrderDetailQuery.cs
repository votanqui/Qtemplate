using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Order;

namespace Qtemplate.Application.Features.Orders.Queries.GetOrderDetail;

public class GetOrderDetailQuery : IRequest<ApiResponse<OrderDto>>
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public bool IsAdmin { get; set; } = false;
}
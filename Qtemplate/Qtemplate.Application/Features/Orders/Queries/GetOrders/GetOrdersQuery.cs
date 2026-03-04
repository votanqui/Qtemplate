using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Order;

namespace Qtemplate.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersQuery : IRequest<ApiResponse<PaginatedResult<OrderDto>>>
{
    public string? Status { get; set; }
    public Guid? UserId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Order;
using Qtemplate.Application.DTOs.User;

namespace Qtemplate.Application.Features.Admin.Users.Queries.GetUserOrders;

public class GetUserOrdersQuery : IRequest<ApiResponse<PaginatedResult<PurchaseHistoryItemDto>>>
{
    public Guid UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
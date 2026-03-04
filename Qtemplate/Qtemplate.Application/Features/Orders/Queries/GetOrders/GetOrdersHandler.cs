using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Order;
using Qtemplate.Application.Mappers;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, ApiResponse<PaginatedResult<OrderDto>>>
{
    private readonly IOrderRepository _orderRepo;
    public GetOrdersHandler(IOrderRepository orderRepo) => _orderRepo = orderRepo;

    public async Task<ApiResponse<PaginatedResult<OrderDto>>> Handle(
        GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _orderRepo.GetAdminListAsync(
            request.Status, request.UserId, request.Search,
            request.Page, request.PageSize);

        return ApiResponse<PaginatedResult<OrderDto>>.Ok(new PaginatedResult<OrderDto>
        {
            Items = items.Select(OrderMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
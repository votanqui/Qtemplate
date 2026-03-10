using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Order;
using Qtemplate.Application.Mappers;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Orders.Queries.GetOrderDetail;

public class GetOrderDetailHandler : IRequestHandler<GetOrderDetailQuery, ApiResponse<OrderDto>>
{
    private readonly IOrderRepository _orderRepo;
    public GetOrderDetailHandler(IOrderRepository orderRepo) => _orderRepo = orderRepo;

    public async Task<ApiResponse<OrderDto>> Handle(
        GetOrderDetailQuery request, CancellationToken cancellationToken)
    {
        var order = !string.IsNullOrEmpty(request.OrderCode)
            ? await _orderRepo.GetByOrderCodeAsync(request.OrderCode)
            : await _orderRepo.GetByIdWithDetailsAsync(request.OrderId);

        if (order is null)
            return ApiResponse<OrderDto>.Fail("Không tìm thấy đơn hàng");

        if (!request.IsAdmin && order.UserId != request.UserId)
            return ApiResponse<OrderDto>.Fail("Không có quyền xem đơn hàng này");

        return ApiResponse<OrderDto>.Ok(OrderMapper.ToDto(order));
    }
}
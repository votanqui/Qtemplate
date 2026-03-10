using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.User;
using Qtemplate.Application.Mappers;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Admin.Users.Queries.GetUserOrders;

public class GetUserOrdersHandler
    : IRequestHandler<GetUserOrdersQuery, ApiResponse<PaginatedResult<PurchaseHistoryItemDto>>>
{
    private readonly IOrderRepository _orderRepo;
    private readonly IUserRepository _userRepo;

    public GetUserOrdersHandler(IOrderRepository orderRepo, IUserRepository userRepo)
    {
        _orderRepo = orderRepo;
        _userRepo = userRepo;
    }

    public async Task<ApiResponse<PaginatedResult<PurchaseHistoryItemDto>>> Handle(
        GetUserOrdersQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId);
        if (user is null)
            return ApiResponse<PaginatedResult<PurchaseHistoryItemDto>>.Fail("Không tìm thấy người dùng");

        var (orders, total) = await _orderRepo.GetPagedByUserIdAsync(
            request.UserId, request.Page, request.PageSize);

        return ApiResponse<PaginatedResult<PurchaseHistoryItemDto>>.Ok(
            new PaginatedResult<PurchaseHistoryItemDto>
            {
                Items = orders.Select(UserMapper.ToPurchaseHistoryDto).ToList(),
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize
            });
    }
}
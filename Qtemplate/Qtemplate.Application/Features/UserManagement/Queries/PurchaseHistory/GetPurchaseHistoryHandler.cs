using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.User;
using Qtemplate.Application.Mappers;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.UserManagement.Queries.PurchaseHistory;

public class GetPurchaseHistoryHandler
    : IRequestHandler<GetPurchaseHistoryQuery, ApiResponse<PaginatedResult<PurchaseHistoryItemDto>>>
{
    private readonly IOrderRepository _orderRepo;
    public GetPurchaseHistoryHandler(IOrderRepository orderRepo) => _orderRepo = orderRepo;

    public async Task<ApiResponse<PaginatedResult<PurchaseHistoryItemDto>>> Handle(
        GetPurchaseHistoryQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _orderRepo.GetPagedByUserIdAsync(
            request.UserId, request.Page, request.PageSize, request.Status); // ← thêm Status

        return ApiResponse<PaginatedResult<PurchaseHistoryItemDto>>.Ok(
            new PaginatedResult<PurchaseHistoryItemDto>
            {
                Items = items.Select(UserMapper.ToPurchaseHistoryDto).ToList(),
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize
            });
    }
}
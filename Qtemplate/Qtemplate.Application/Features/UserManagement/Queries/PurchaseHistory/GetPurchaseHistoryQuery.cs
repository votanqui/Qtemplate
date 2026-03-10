using MediatR;
using Qtemplate.Application.DTOs.User;
using Qtemplate.Application.DTOs;

public class GetPurchaseHistoryQuery : IRequest<ApiResponse<PaginatedResult<PurchaseHistoryItemDto>>>
{
    public Guid UserId { get; set; }
    public string? Status { get; set; } // ← thêm
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
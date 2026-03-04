using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Order;
using Qtemplate.Application.DTOs.User;
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
            request.UserId, request.Page, request.PageSize);

        var isPaidStatuses = new[] { "Paid", "Completed" };

        var dtos = items.Select(o =>
        {
            var isPaid = isPaidStatuses.Contains(o.Status);

            return new PurchaseHistoryItemDto
            {
                OrderId = o.Id,
                OrderCode = o.OrderCode,
                TotalAmount = o.TotalAmount,
                DiscountAmount = o.DiscountAmount,
                FinalAmount = o.FinalAmount,
                CouponCode = o.CouponCode,
                Status = o.Status,
                CancelReason = o.CancelReason,
                PaymentStatus = o.Payment?.Status,
                BankCode = o.Payment?.BankCode,
                PaidAt = o.Payment?.PaidAt,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    TemplateId = i.TemplateId,
                    TemplateName = i.TemplateName,
                    OriginalPrice = i.OriginalPrice,
                    Price = i.Price,
                    ThumbnailUrl = i.Template?.ThumbnailUrl,
                    TemplateSlug = i.Template?.Slug,
                    DownloadUrl = isPaid && i.Template?.Slug != null
                        ? $"/api/templates/{i.Template.Slug}/download"
                        : null
                }).ToList()
            };
        }).ToList();

        return ApiResponse<PaginatedResult<PurchaseHistoryItemDto>>.Ok(
            new PaginatedResult<PurchaseHistoryItemDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize
            });
    }
}
using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Order;
using Qtemplate.Application.DTOs.payments;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Payments.Queries.GetPaymentStatus;

public class GetPaymentStatusHandler
    : IRequestHandler<GetPaymentStatusQuery, ApiResponse<PaymentStatusDto>>
{
    private readonly IOrderRepository _orderRepo;
    private readonly IPaymentRepository _paymentRepo;

    public GetPaymentStatusHandler(IOrderRepository orderRepo, IPaymentRepository paymentRepo)
    {
        _orderRepo = orderRepo;
        _paymentRepo = paymentRepo;
    }

    public async Task<ApiResponse<PaymentStatusDto>> Handle(
        GetPaymentStatusQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepo.GetByIdWithDetailsAsync(request.OrderId);
        if (order is null)
            return ApiResponse<PaymentStatusDto>.Fail("Không tìm thấy đơn hàng");

        if (order.UserId != request.UserId)
            return ApiResponse<PaymentStatusDto>.Fail("Không có quyền xem đơn này");

        var payment = await _paymentRepo.GetByOrderIdAsync(order.Id);
        var isPaid = order.Status is "Paid" or "Completed";

        var downloads = isPaid
            ? order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                TemplateId = i.TemplateId,
                TemplateName = i.TemplateName,
                OriginalPrice = i.OriginalPrice,
                Price = i.Price,
                ThumbnailUrl = i.Template?.ThumbnailUrl,
                TemplateSlug = i.Template?.Slug,
                DownloadUrl = i.Template?.Slug != null
                    ? $"/api/templates/{i.Template.Slug}/download"
                    : null
            }).ToList()
            : new List<OrderItemDto>();

        return ApiResponse<PaymentStatusDto>.Ok(new PaymentStatusDto
        {
            OrderCode = order.OrderCode,
            OrderStatus = order.Status,
            PaymentStatus = payment?.Status ?? "Pending",
            FinalAmount = order.FinalAmount,
            PaidAmount = payment?.Amount,
            BankCode = payment?.BankCode,
            PaidAt = payment?.PaidAt,
            IsPaid = isPaid,
            Downloads = downloads
        });
    }
}
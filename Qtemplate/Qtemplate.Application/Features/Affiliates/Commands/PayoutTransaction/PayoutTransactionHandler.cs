using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Affiliates.Commands.PayoutTransaction;

public class PayoutTransactionHandler : IRequestHandler<PayoutTransactionCommand, ApiResponse<bool>>
{
    private readonly IAffiliateRepository _affiliateRepo;
    private readonly INotificationService _notifService;

    public PayoutTransactionHandler(
        IAffiliateRepository affiliateRepo,
        INotificationService notifService)
    {
        _affiliateRepo = affiliateRepo;
        _notifService = notifService;
    }

    public async Task<ApiResponse<bool>> Handle(
        PayoutTransactionCommand request, CancellationToken cancellationToken)
    {
        var tx = await _affiliateRepo.GetTransactionByIdAsync(request.TransactionId);
        if (tx is null)
            return ApiResponse<bool>.Fail("Không tìm thấy transaction");

        if (tx.Status == "Paid")
            return ApiResponse<bool>.Fail("Transaction đã được thanh toán rồi");

        var affiliate = await _affiliateRepo.GetByIdAsync(tx.AffiliateId);
        if (affiliate is null)
            return ApiResponse<bool>.Fail("Không tìm thấy affiliate");

        tx.Status = "Paid";
        tx.PaidAt = DateTime.UtcNow;
        await _affiliateRepo.UpdateTransactionAsync(tx);

        affiliate.PendingAmount = Math.Max(0, affiliate.PendingAmount - tx.Commission);
        affiliate.PaidAmount += tx.Commission;
        await _affiliateRepo.UpdateAsync(affiliate);

        // 🔔 Noti cho affiliate biết hoa hồng đã được thanh toán
        await _notifService.SendToUserAsync(
            affiliate.UserId,
            "Hoa hồng đã được thanh toán 💰",
            $"Bạn vừa nhận được {tx.Commission:N0}₫ hoa hồng từ đơn hàng {tx.Order?.OrderCode ?? tx.OrderId.ToString()[..8]}.",
            type: "Success",
            redirectUrl: "/dashboard/affiliate");

        return ApiResponse<bool>.Ok(true, "Đã thanh toán hoa hồng thành công");
    }
}
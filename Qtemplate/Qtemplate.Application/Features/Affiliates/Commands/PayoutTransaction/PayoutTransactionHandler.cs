using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Affiliates.Commands.PayoutTransaction;

public class PayoutTransactionHandler : IRequestHandler<PayoutTransactionCommand, ApiResponse<bool>>
{
    private readonly IAffiliateRepository _affiliateRepo;

    public PayoutTransactionHandler(IAffiliateRepository affiliateRepo)
        => _affiliateRepo = affiliateRepo;

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

        return ApiResponse<bool>.Ok(true, "Đã thanh toán hoa hồng thành công");
    }
}
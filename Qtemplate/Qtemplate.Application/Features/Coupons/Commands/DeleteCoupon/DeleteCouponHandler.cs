using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Coupons.Commands.DeleteCoupon;

public class DeleteCouponHandler : IRequestHandler<DeleteCouponCommand, ApiResponse<object>>
{
    private readonly ICouponRepository _couponRepo;
    private readonly IAuditLogService _auditLogService;

    public DeleteCouponHandler(ICouponRepository couponRepo, IAuditLogService auditLogService)
    {
        _couponRepo = couponRepo;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<object>> Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _couponRepo.GetByIdAsync(request.Id);
        if (coupon is null) return ApiResponse<object>.Fail("Không tìm thấy coupon");

        await _couponRepo.DeleteAsync(coupon);

        await _auditLogService.LogAsync(
            userId: request.AdminId, userEmail: request.AdminEmail,
            action: "DeleteCoupon", entityName: "Coupon",
            entityId: request.Id.ToString(),
            oldValues: new { coupon.Code },
            ipAddress: request.IpAddress);

        return ApiResponse<object>.Ok(null!, "Đã xóa coupon");
    }
}
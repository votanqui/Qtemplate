using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Coupons.Commands.UpdateCoupon;

public class UpdateCouponHandler : IRequestHandler<UpdateCouponCommand, ApiResponse<object>>
{
    private readonly ICouponRepository _couponRepo;
    private readonly IAuditLogService _auditLogService;

    public UpdateCouponHandler(ICouponRepository couponRepo, IAuditLogService auditLogService)
    {
        _couponRepo = couponRepo;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<object>> Handle(UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _couponRepo.GetByIdAsync(request.Id);
        if (coupon is null) return ApiResponse<object>.Fail("Không tìm thấy coupon");

        var old = new { coupon.Value, coupon.IsActive, coupon.ExpiredAt };
        var dto = request.Dto;

        coupon.Value = dto.Value;
        coupon.MinOrderAmount = dto.MinOrderAmount;
        coupon.MaxDiscountAmount = dto.MaxDiscountAmount;
        coupon.UsageLimit = dto.UsageLimit;
        coupon.IsActive = dto.IsActive;
        coupon.StartAt = dto.StartAt;
        coupon.ExpiredAt = dto.ExpiredAt;

        await _couponRepo.UpdateAsync(coupon);

        await _auditLogService.LogAsync(
            userId: request.AdminId, userEmail: request.AdminEmail,
            action: "UpdateCoupon", entityName: "Coupon",
            entityId: coupon.Id.ToString(),
            oldValues: old,
            newValues: new { coupon.Value, coupon.IsActive, coupon.ExpiredAt },
            ipAddress: request.IpAddress);

        return ApiResponse<object>.Ok(null!, "Cập nhật coupon thành công");
    }
}
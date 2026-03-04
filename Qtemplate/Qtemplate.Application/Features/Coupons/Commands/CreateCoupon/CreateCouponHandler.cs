using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Coupons.Commands.CreateCoupon;

public class CreateCouponHandler : IRequestHandler<CreateCouponCommand, ApiResponse<int>>
{
    private readonly ICouponRepository _couponRepo;
    private readonly IAuditLogService _auditLogService;

    public CreateCouponHandler(ICouponRepository couponRepo, IAuditLogService auditLogService)
    {
        _couponRepo = couponRepo;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<int>> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        if (await _couponRepo.GetByCodeAsync(dto.Code) is not null)
            return ApiResponse<int>.Fail("Mã coupon đã tồn tại");

        if (dto.Type != "Percent" && dto.Type != "Fixed")
            return ApiResponse<int>.Fail("Type phải là Percent hoặc Fixed");

        if (dto.Type == "Percent" && (dto.Value <= 0 || dto.Value > 100))
            return ApiResponse<int>.Fail("Giá trị % phải từ 1-100");

        if (dto.Type == "Fixed" && dto.Value <= 0)
            return ApiResponse<int>.Fail("Giá trị giảm phải > 0");

        var coupon = new Coupon
        {
            Code = dto.Code.Trim().ToUpper(),
            Type = dto.Type,
            Value = dto.Value,
            MinOrderAmount = dto.MinOrderAmount,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            UsageLimit = dto.UsageLimit,
            IsActive = true,
            StartAt = dto.StartAt,
            ExpiredAt = dto.ExpiredAt,
            CreatedAt = DateTime.UtcNow
        };

        await _couponRepo.AddAsync(coupon);

        await _auditLogService.LogAsync(
            userId: request.AdminId, userEmail: request.AdminEmail,
            action: "CreateCoupon", entityName: "Coupon",
            entityId: coupon.Id.ToString(),
            newValues: new { coupon.Code, coupon.Type, coupon.Value },
            ipAddress: request.IpAddress);

        return ApiResponse<int>.Ok(coupon.Id, "Tạo coupon thành công");
    }
}
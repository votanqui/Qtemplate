using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Coupon;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Coupons.Queries.GetCoupons;

public class GetCouponsHandler : IRequestHandler<GetCouponsQuery, ApiResponse<PaginatedResult<CouponDto>>>
{
    private readonly ICouponRepository _couponRepo;
    public GetCouponsHandler(ICouponRepository couponRepo) => _couponRepo = couponRepo;

    public async Task<ApiResponse<PaginatedResult<CouponDto>>> Handle(
        GetCouponsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _couponRepo.GetAdminListAsync(request.IsActive, request.Page, request.PageSize);

        var dtos = items.Select(c => new CouponDto
        {
            Id = c.Id,
            Code = c.Code,
            Type = c.Type,
            Value = c.Value,
            MinOrderAmount = c.MinOrderAmount,
            MaxDiscountAmount = c.MaxDiscountAmount,
            UsageLimit = c.UsageLimit,
            UsedCount = c.UsedCount,
            IsActive = c.IsActive,
            StartAt = c.StartAt,
            ExpiredAt = c.ExpiredAt,
            CreatedAt = c.CreatedAt
        }).ToList();

        return ApiResponse<PaginatedResult<CouponDto>>.Ok(new PaginatedResult<CouponDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Coupon;

namespace Qtemplate.Application.Features.Coupons.Queries.GetCoupons;

public class GetCouponsQuery : IRequest<ApiResponse<PaginatedResult<CouponDto>>>
{
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
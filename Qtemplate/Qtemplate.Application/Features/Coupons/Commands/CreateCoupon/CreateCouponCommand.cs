using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Coupon;

namespace Qtemplate.Application.Features.Coupons.Commands.CreateCoupon;

public class CreateCouponCommand : IRequest<ApiResponse<int>>
{
    public CreateCouponDto Dto { get; set; } = null!;
    public string? AdminId { get; set; }
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}
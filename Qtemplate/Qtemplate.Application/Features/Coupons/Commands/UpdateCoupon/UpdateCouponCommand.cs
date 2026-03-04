using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Coupon;

namespace Qtemplate.Application.Features.Coupons.Commands.UpdateCoupon;

public class UpdateCouponCommand : IRequest<ApiResponse<object>>
{
    public int Id { get; set; }
    public UpdateCouponDto Dto { get; set; } = null!;
    public string? AdminId { get; set; }
    public string? AdminEmail { get; set; }
    public string? IpAddress { get; set; }
}
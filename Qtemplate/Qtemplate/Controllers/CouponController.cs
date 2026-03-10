using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.Features.Coupons.Queries.GetCoupons;

namespace Qtemplate.Controllers;

[ApiController]
[Route("api/coupons")]
public class CouponController : ControllerBase
{
    private readonly IMediator _mediator;
    public CouponController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// GET /api/coupons
    /// Trả về danh sách coupon public đang active, chưa hết hạn, còn lượt dùng
    /// Không cần đăng nhập — user xem để biết có mã nào đang chạy
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublic()
    {
        var result = await _mediator.Send(new GetCouponsQuery
        {
            IsActive = true,
            PageSize = 50,
            Page = 1,
        });
        return Ok(result);
    }
}
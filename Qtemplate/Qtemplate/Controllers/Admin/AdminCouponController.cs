using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.Coupon;
using Qtemplate.Application.Features.Coupons.Commands.CreateCoupon;
using Qtemplate.Application.Features.Coupons.Commands.DeleteCoupon;
using Qtemplate.Application.Features.Coupons.Commands.UpdateCoupon;
using Qtemplate.Application.Features.Coupons.Queries.GetCoupons;
using System.Security.Claims;

namespace Qtemplate.Controllers.Admin;

[ApiController]
[Route("api/admin/coupons")]
[Authorize(Roles = "Admin")]
public class AdminCouponController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminCouponController(IMediator mediator) => _mediator = mediator;

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
    private string? GetUserEmail() => User.FindFirstValue(ClaimTypes.Email);
    private string GetIpAddress() =>
        Request.Headers.ContainsKey("X-Forwarded-For")
            ? Request.Headers["X-Forwarded-For"].ToString()
            : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    // GET /api/admin/coupons?isActive=true&page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetCouponsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // POST /api/admin/coupons
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCouponDto dto)
    {
        var result = await _mediator.Send(new CreateCouponCommand
        {
            Dto = dto,
            AdminId = GetUserId(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // PUT /api/admin/coupons/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCouponDto dto)
    {
        var result = await _mediator.Send(new UpdateCouponCommand
        {
            Id = id,
            Dto = dto,
            AdminId = GetUserId(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // DELETE /api/admin/coupons/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteCouponCommand
        {
            Id = id,
            AdminId = GetUserId(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
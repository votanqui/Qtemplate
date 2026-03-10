using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.Order;
using Qtemplate.Application.Features.Orders.Commands.ApplyCoupon;
using Qtemplate.Application.Features.Orders.Commands.CancelOrder;
using Qtemplate.Application.Features.Orders.Commands.CreateOrder;
using Qtemplate.Application.Features.Orders.Queries.GetOrderDetail;
using Qtemplate.Application.Features.Payments.Commands.CreatePayment;
using Qtemplate.Application.Features.Payments.Queries.GetPaymentStatus;
using Qtemplate.Application.Features.UserManagement.Queries.PurchaseHistory;
using System.Security.Claims;

namespace Qtemplate.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;
    public OrderController(IMediator mediator) => _mediator = mediator;

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    // POST /api/orders
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var result = await _mediator.Send(new CreateOrderCommand
        {
            UserId = GetUserId(),
            TemplateIds = dto.TemplateIds,
            CouponCode = dto.CouponCode,
            AffiliateCode = dto.AffiliateCode,
            Note = dto.Note
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // GET /api/orders/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var result = await _mediator.Send(new GetOrderDetailQuery
        {
            OrderId = id,
            UserId = GetUserId()
        });
        return result.Success ? Ok(result) : NotFound(result);
    }
    [HttpGet("code/{orderCode}")]
    public async Task<IActionResult> GetDetailByCode(string orderCode)
    {
        var result = await _mediator.Send(new GetOrderDetailQuery
        {
            OrderCode = orderCode,
            UserId = GetUserId()
        });
        return result.Success ? Ok(result) : NotFound(result);
    }
    // POST /api/orders/apply-coupon
    [HttpPost("apply-coupon")]
    public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponDto dto)
    {
        var result = await _mediator.Send(new ApplyCouponQuery
        {
            CouponCode = dto.CouponCode,
            TemplateIds = dto.TemplateIds
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // POST /api/orders/{id}/payment
    [HttpPost("{id:guid}/payment")]
    public async Task<IActionResult> CreatePayment(Guid id)
    {
        var result = await _mediator.Send(new CreatePaymentCommand
        {
            OrderId = id,
            UserId = GetUserId()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // POST /api/orders/{id}/cancel
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelReasonDto dto)
    {
        var result = await _mediator.Send(new CancelOrderCommand
        {
            OrderId = id,
            UserId = GetUserId(),
            CancelReason = dto.Reason
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [HttpGet("{id:guid}/payment-status")]
    public async Task<IActionResult> GetPaymentStatus(Guid id)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new GetPaymentStatusQuery
        {
            OrderId = id,
            UserId = userId
        });

        return result.Success ? Ok(result) : NotFound(result);
    }
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.Order;
using Qtemplate.Application.Features.Orders.Commands.CancelOrder;
using Qtemplate.Application.Features.Orders.Queries.GetOrderDetail;
using Qtemplate.Application.Features.Orders.Queries.GetOrders;
using System.Security.Claims;

namespace Qtemplate.Controllers.Admin;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrderController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminOrderController(IMediator mediator) => _mediator = mediator;

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    // GET /api/admin/orders?status=&search=&page=&pageSize=
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetOrdersQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // GET /api/admin/orders/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var result = await _mediator.Send(new GetOrderDetailQuery
        {
            OrderId = id,
            UserId = GetUserId(),
            IsAdmin = true
        });
        return result.Success ? Ok(result) : NotFound(result);
    }

    // POST /api/admin/orders/{id}/cancel
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelReasonDto dto)
    {
        var result = await _mediator.Send(new CancelOrderCommand
        {
            OrderId = id,
            UserId = GetUserId(),
            IsAdmin = true,
            CancelReason = dto.Reason
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}


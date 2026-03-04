using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.Ticket;
using Qtemplate.Application.Features.Tickets.Commands.CreateTicket;
using Qtemplate.Application.Features.Tickets.Commands.ReplyTicket;
using Qtemplate.Application.Features.Tickets.Queries.GetMyTickets;
using Qtemplate.Application.Features.Tickets.Queries.GetTicketDetail;

namespace Qtemplate.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketController : ControllerBase
{
    private readonly IMediator _mediator;
    public TicketController(IMediator mediator) => _mediator = mediator;

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    // GET /api/tickets?page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetMyTickets(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new GetMyTicketsQuery
        {
            UserId = userId,
            Page = page,
            PageSize = pageSize
        });
        return Ok(result);
    }

    // GET /api/tickets/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDetail(int id)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new GetTicketDetailQuery
        {
            TicketId = id,
            UserId = userId,
            IsAdmin = false
        });
        return result.Success ? Ok(result) : NotFound(result);
    }

    // POST /api/tickets
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketDto dto)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new CreateTicketCommand
        {
            UserId = userId,
            Subject = dto.Subject,
            Message = dto.Message,
            TemplateId = dto.TemplateId
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // POST /api/tickets/{id}/reply
    [HttpPost("{id:int}/reply")]
    public async Task<IActionResult> Reply(int id, [FromBody] ReplyTicketDto dto)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new ReplyTicketCommand
        {
            TicketId = id,
            UserId = userId,
            Message = dto.Message,
            AttachmentUrl = dto.AttachmentUrl,
            IsFromAdmin = false
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
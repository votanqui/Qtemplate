using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.Ticket;
using Qtemplate.Application.Features.Tickets.Commands.AssignTicket;
using Qtemplate.Application.Features.Tickets.Commands.ChangeTicketPriority;
using Qtemplate.Application.Features.Tickets.Commands.ChangeTicketStatus;
using Qtemplate.Application.Features.Tickets.Commands.ReplyTicket;
using Qtemplate.Application.Features.Tickets.Queries.GetAdminTickets;
using Qtemplate.Application.Features.Tickets.Queries.GetTicketDetail;

namespace Qtemplate.Controllers.Admin;

[ApiController]
[Route("api/admin/tickets")]
[Authorize(Roles = "Admin")]
public class AdminTicketController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminTicketController(IMediator mediator) => _mediator = mediator;

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    // GET /api/admin/tickets?status=Open&priority=Urgent&page=1
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAdminTicketsQuery
        {
            Status = status,
            Priority = priority,
            Page = page,
            PageSize = pageSize
        });
        return Ok(result);
    }

    // GET /api/admin/tickets/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDetail(int id)
    {
        var result = await _mediator.Send(new GetTicketDetailQuery
        {
            TicketId = id,
            UserId = GetUserId(),
            IsAdmin = true
        });
        return result.Success ? Ok(result) : NotFound(result);
    }

    // POST /api/admin/tickets/{id}/reply
    [HttpPost("{id:int}/reply")]
    public async Task<IActionResult> Reply(int id, [FromBody] ReplyTicketDto dto)
    {
        var result = await _mediator.Send(new ReplyTicketCommand
        {
            TicketId = id,
            UserId = GetUserId(),
            Message = dto.Message,
            AttachmentUrl = dto.AttachmentUrl,
            IsFromAdmin = true
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // PATCH /api/admin/tickets/{id}/status
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeTicketStatusDto dto)
    {
        var result = await _mediator.Send(new ChangeTicketStatusCommand
        {
            TicketId = id,
            Status = dto.Status
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // PATCH /api/admin/tickets/{id}/assign
    [HttpPatch("{id:int}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignTicketDto dto)
    {
        var result = await _mediator.Send(new AssignTicketCommand
        {
            TicketId = id,
            AssignedTo = dto.AssignedTo
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [HttpPatch("{id:int}/priority")]
    public async Task<IActionResult> ChangePriority(int id, [FromBody] ChangeTicketPriorityDto dto)
    {
        var result = await _mediator.Send(new ChangeTicketPriorityCommand
        {
            TicketId = id,
            Priority = dto.Priority
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
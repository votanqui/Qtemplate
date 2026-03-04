using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Tickets.Commands.AssignTicket;

public class AssignTicketHandler : IRequestHandler<AssignTicketCommand, ApiResponse<object>>
{
    private readonly ITicketRepository _ticketRepo;

    public AssignTicketHandler(ITicketRepository ticketRepo) => _ticketRepo = ticketRepo;

    public async Task<ApiResponse<object>> Handle(
        AssignTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepo.GetByIdAsync(request.TicketId);
        if (ticket is null)
            return ApiResponse<object>.Fail("Không tìm thấy ticket");

        ticket.AssignedTo = request.AssignedTo;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (ticket.Status == "Open")
            ticket.Status = "InProgress";

        await _ticketRepo.UpdateAsync(ticket);
        return ApiResponse<object>.Ok(null!, "Đã assign ticket");
    }
}
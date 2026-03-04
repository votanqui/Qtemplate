using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Tickets.Commands.ChangeTicketPriority;

public class ChangeTicketPriorityHandler
    : IRequestHandler<ChangeTicketPriorityCommand, ApiResponse<object>>
{
    private readonly ITicketRepository _ticketRepo;

    public ChangeTicketPriorityHandler(ITicketRepository ticketRepo)
        => _ticketRepo = ticketRepo;

    public async Task<ApiResponse<object>> Handle(
        ChangeTicketPriorityCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepo.GetByIdAsync(request.TicketId);
        if (ticket is null)
            return ApiResponse<object>.Fail("Không tìm thấy ticket");

        var valid = new[] { "Low", "Normal", "High", "Urgent" };
        if (!valid.Contains(request.Priority))
            return ApiResponse<object>.Fail("Priority không hợp lệ. Chỉ chấp nhận: Low, Normal, High, Urgent");

        ticket.Priority = request.Priority;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _ticketRepo.UpdateAsync(ticket);
        return ApiResponse<object>.Ok(null!, $"Đã cập nhật priority thành {request.Priority}");
    }
}
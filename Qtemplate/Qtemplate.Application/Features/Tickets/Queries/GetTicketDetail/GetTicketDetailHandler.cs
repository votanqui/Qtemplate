using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Ticket;
using Qtemplate.Application.Mappers;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Tickets.Queries.GetTicketDetail;

public class GetTicketDetailHandler
    : IRequestHandler<GetTicketDetailQuery, ApiResponse<TicketDetailDto>>
{
    private readonly ITicketRepository _ticketRepo;

    public GetTicketDetailHandler(ITicketRepository ticketRepo) => _ticketRepo = ticketRepo;

    public async Task<ApiResponse<TicketDetailDto>> Handle(
        GetTicketDetailQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepo.GetByIdWithRepliesAsync(request.TicketId);
        if (ticket is null)
            return ApiResponse<TicketDetailDto>.Fail("Không tìm thấy ticket");

        // User chỉ xem ticket của mình
        if (!request.IsAdmin && ticket.UserId != request.UserId)
            return ApiResponse<TicketDetailDto>.Fail("Không có quyền xem ticket này");

        return ApiResponse<TicketDetailDto>.Ok(TicketMapper.ToDetailDto(ticket));
    }
}
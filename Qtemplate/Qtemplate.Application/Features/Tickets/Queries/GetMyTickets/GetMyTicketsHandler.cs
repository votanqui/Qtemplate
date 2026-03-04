using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Ticket;
using Qtemplate.Application.Mappers;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Tickets.Queries.GetMyTickets;

public class GetMyTicketsHandler
    : IRequestHandler<GetMyTicketsQuery, ApiResponse<PaginatedResult<TicketDto>>>
{
    private readonly ITicketRepository _ticketRepo;

    public GetMyTicketsHandler(ITicketRepository ticketRepo) => _ticketRepo = ticketRepo;

    public async Task<ApiResponse<PaginatedResult<TicketDto>>> Handle(
        GetMyTicketsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _ticketRepo.GetByUserIdAsync(
            request.UserId, request.Page, request.PageSize);

        return ApiResponse<PaginatedResult<TicketDto>>.Ok(new PaginatedResult<TicketDto>
        {
            Items = items.Select(TicketMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Ticket;
using Qtemplate.Application.Mappers;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Tickets.Commands.CreateTicket;

public class CreateTicketHandler : IRequestHandler<CreateTicketCommand, ApiResponse<TicketDto>>
{
    private readonly ITicketRepository _ticketRepo;
    private readonly ITemplateRepository _templateRepo;
    private readonly IAiModerationService _aiService;

    public CreateTicketHandler(
        ITicketRepository ticketRepo,
        ITemplateRepository templateRepo,
        IAiModerationService aiService)
    {
        _ticketRepo = ticketRepo;
        _templateRepo = templateRepo;
        _aiService = aiService;
    }

    public async Task<ApiResponse<TicketDto>> Handle(
        CreateTicketCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Subject))
            return ApiResponse<TicketDto>.Fail("Subject không được để trống");

        if (string.IsNullOrWhiteSpace(request.Message))
            return ApiResponse<TicketDto>.Fail("Message không được để trống");

        if (request.TemplateId.HasValue)
        {
            var template = await _templateRepo.GetByIdAsync(request.TemplateId.Value);
            if (template is null)
                return ApiResponse<TicketDto>.Fail("Template không tồn tại");
            if (template.Status != "Published")
                return ApiResponse<TicketDto>.Fail("Template không khả dụng");
        }

        // ── Sync rule-based (~0ms, không gọi API) ──
        var priorityResult = _aiService.ClassifyTicketPrioritySync(
            request.Subject, request.Message);

        var ticketCode = await _ticketRepo.GenerateTicketCodeAsync();

        var ticket = new SupportTicket
        {
            UserId = request.UserId,
            TemplateId = request.TemplateId,
            TicketCode = ticketCode,
            Subject = request.Subject.Trim(),
            Message = request.Message.Trim(),
            Priority = priorityResult.Priority,
            AiPriorityReason = priorityResult.Reason,
            Status = "Open",
            CreatedAt = DateTime.UtcNow
        };

        await _ticketRepo.AddAsync(ticket);

        return ApiResponse<TicketDto>.Ok(
            TicketMapper.ToDto(ticket), "Tạo ticket thành công");
    }
}
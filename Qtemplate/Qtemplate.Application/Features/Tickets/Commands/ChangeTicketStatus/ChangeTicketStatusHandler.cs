using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Domain.Messages;

namespace Qtemplate.Application.Features.Tickets.Commands.ChangeTicketStatus;

public class ChangeTicketStatusHandler : IRequestHandler<ChangeTicketStatusCommand, ApiResponse<object>>
{
    private readonly ITicketRepository _ticketRepo;
    private readonly IUserRepository _userRepo;
    private readonly IEmailSender _emailSender;

    public ChangeTicketStatusHandler(
        ITicketRepository ticketRepo,
        IUserRepository userRepo,
        IEmailSender emailSender)
    {
        _ticketRepo = ticketRepo;
        _userRepo = userRepo;
        _emailSender = emailSender;
    }

    public async Task<ApiResponse<object>> Handle(
        ChangeTicketStatusCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepo.GetByIdAsync(request.TicketId);
        if (ticket is null)
            return ApiResponse<object>.Fail("Không tìm thấy ticket");

        var validStatuses = new[] { "Open", "InProgress", "Closed" };
        if (!validStatuses.Contains(request.Status))
            return ApiResponse<object>.Fail("Status không hợp lệ");

        var oldStatus = ticket.Status;
        ticket.Status = request.Status;
        ticket.ClosedAt = request.Status == "Closed" ? DateTime.UtcNow : null;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _ticketRepo.UpdateAsync(ticket);

        // Gửi email khi status thay đổi (chỉ notify khi khác status cũ)
        if (oldStatus != request.Status)
        {
            var user = await _userRepo.GetByIdAsync(ticket.UserId);
            if (user is not null)
            {
                var statusLabel = request.Status switch
                {
                    "InProgress" => "đang xử lý",
                    "Closed" => "đã đóng",
                    "Open" => "đã mở lại",
                    _ => request.Status
                };

                _ = _emailSender.SendAsync(new SendEmailMessage
                {
                    To = user.Email,
                    Subject = $"[{ticket.TicketCode}] Ticket {statusLabel}",
                    Body = EmailTemplates.TicketStatusChanged(
                        user.FullName,
                        ticket.TicketCode,
                        ticket.Subject,
                        statusLabel),
                    Template = "TicketStatusChanged"
                });
            }
        }

        return ApiResponse<object>.Ok(null!, $"Đã cập nhật status thành {request.Status}");
    }
}
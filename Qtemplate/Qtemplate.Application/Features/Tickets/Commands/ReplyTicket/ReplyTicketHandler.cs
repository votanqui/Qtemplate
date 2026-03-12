using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Ticket;
using Qtemplate.Application.Mappers;
using Qtemplate.Application.Services;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Domain.Messages;

namespace Qtemplate.Application.Features.Tickets.Commands.ReplyTicket;

public class ReplyTicketHandler : IRequestHandler<ReplyTicketCommand, ApiResponse<TicketReplyDto>>
{
    private readonly ITicketRepository _ticketRepo;
    private readonly IUserRepository _userRepo;
    private readonly IEmailSender _emailSender;
    private readonly INotificationService _notifService;

    public ReplyTicketHandler(
        ITicketRepository ticketRepo,
        IUserRepository userRepo,
        IEmailSender emailSender,
        INotificationService notifService)
    {
        _ticketRepo = ticketRepo;
        _userRepo = userRepo;
        _emailSender = emailSender;
        _notifService = notifService;
    }

    public async Task<ApiResponse<TicketReplyDto>> Handle(
        ReplyTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepo.GetByIdAsync(request.TicketId);
        if (ticket is null)
            return ApiResponse<TicketReplyDto>.Fail("Không tìm thấy ticket");

        if (!request.IsFromAdmin && ticket.UserId != request.UserId)
            return ApiResponse<TicketReplyDto>.Fail("Không có quyền reply ticket này");

        if (ticket.Status == "Closed")
            return ApiResponse<TicketReplyDto>.Fail("Ticket đã đóng, không thể reply");

        if (string.IsNullOrWhiteSpace(request.Message))
            return ApiResponse<TicketReplyDto>.Fail("Message không được để trống");

        var reply = new TicketReply
        {
            TicketId = request.TicketId,
            UserId = request.UserId,
            Message = request.Message.Trim(),
            IsFromAdmin = request.IsFromAdmin,
            AttachmentUrl = request.AttachmentUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _ticketRepo.AddReplyAsync(reply);

        if (request.IsFromAdmin && ticket.Status == "Open")
        {
            ticket.Status = "InProgress";
            ticket.UpdatedAt = DateTime.UtcNow;
            await _ticketRepo.UpdateAsync(ticket);
        }
        else
        {
            ticket.UpdatedAt = DateTime.UtcNow;
            await _ticketRepo.UpdateAsync(ticket);
        }

        // 🔔 Chỉ gửi noti khi ADMIN reply → notify user
        if (request.IsFromAdmin)
        {
            await _notifService.SendToUserAsync(
                ticket.UserId,
                "Ticket của bạn có phản hồi mới",
                $"[{ticket.TicketCode}] {ticket.Subject} vừa được admin phản hồi.",
                type: "Info",
                redirectUrl: $"/dashboard/tickets/{ticket.Id}");

            var user = await _userRepo.GetByIdAsync(ticket.UserId);
            if (user is not null)
            {
                _ = _emailSender.SendAsync(new SendEmailMessage
                {
                    To = user.Email,
                    Subject = $"[{ticket.TicketCode}] Ticket của bạn có phản hồi mới",
                    Body = EmailTemplates.TicketReply(
                        user.FullName,
                        ticket.TicketCode,
                        ticket.Subject,
                        request.Message.Trim()),
                    Template = "TicketReply"
                });
            }
        }

        return ApiResponse<TicketReplyDto>.Ok(
            TicketMapper.ToReplyDto(reply), "Đã gửi reply");
    }
}
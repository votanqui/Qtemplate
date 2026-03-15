// File: Qtemplate.Application/Features/Orders/Commands/UpdateOrderStatus/UpdateOrderStatusHandler.cs

using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Domain.Messages;

namespace Qtemplate.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand, ApiResponse<object>>
{
    private readonly IOrderRepository _orderRepo;
    private readonly IUserRepository _userRepo;
    private readonly ITemplateRepository _templateRepo;
    private readonly IAuditLogService _auditLog;
    private readonly INotificationService _notifService;
    private readonly IEmailSender _emailSender;

    public UpdateOrderStatusHandler(
        IOrderRepository orderRepo,
        IUserRepository userRepo,
        ITemplateRepository templateRepo,
        IAuditLogService auditLog,
        INotificationService notifService,
        IEmailSender emailSender)
    {
        _orderRepo = orderRepo;
        _userRepo = userRepo;
        _templateRepo = templateRepo;
        _auditLog = auditLog;
        _notifService = notifService;
        _emailSender = emailSender;
    }

    private static readonly HashSet<string> AllowedTransitions = new(StringComparer.OrdinalIgnoreCase)
    {
        "Paid",
        "Completed"
    };

    public async Task<ApiResponse<object>> Handle(
        UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        // ── Validate status ───────────────────────────────────────────────────
        if (!AllowedTransitions.Contains(request.NewStatus))
            return ApiResponse<object>.Fail(
                $"Trạng thái không hợp lệ. Chỉ chấp nhận: {string.Join(", ", AllowedTransitions)}");

        // ── Lấy order ─────────────────────────────────────────────────────────
        var order = await _orderRepo.GetByIdWithDetailsAsync(request.OrderId);
        if (order is null)
            return ApiResponse<object>.Fail("Không tìm thấy đơn hàng");

        // ── Validate transition ───────────────────────────────────────────────
        var error = ValidateTransition(order.Status, request.NewStatus);
        if (error is not null)
            return ApiResponse<object>.Fail(error);

        var oldStatus = order.Status;

        // ── Cập nhật order ────────────────────────────────────────────────────
        order.Status = request.NewStatus;
        order.Note = string.IsNullOrWhiteSpace(request.Note) ? order.Note : request.Note;
        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepo.UpdateAsync(order);

        // ── Khi chuyển sang Paid: tăng SalesCount ────────────────────────────
        if (request.NewStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var item in order.Items)
            {
                var template = await _templateRepo.GetByIdAsync(item.TemplateId);
                if (template is not null)
                {
                    template.SalesCount++;
                    await _templateRepo.UpdateAsync(template);
                }
            }
        }

        // ── Audit log ─────────────────────────────────────────────────────────
        await _auditLog.LogAsync(
            userId: request.AdminId.ToString(),
            userEmail: request.AdminEmail,
            action: "AdminUpdateOrderStatus",
            entityName: "Order",
            entityId: order.Id.ToString(),
            oldValues: new { Status = oldStatus },
            newValues: new { Status = request.NewStatus, Note = request.Note });

        // ── Notification + email cho user ─────────────────────────────────────
        var (notiTitle, notiMsg, notiType) = request.NewStatus switch
        {
            "Paid" => (
                "Đơn hàng đã được xác nhận thanh toán",
                $"Đơn hàng {order.OrderCode} đã được admin xác nhận thanh toán thủ công. Bạn có thể tải xuống ngay.",
                "Success"),
            "Completed" => (
                "Đơn hàng hoàn tất",
                $"Đơn hàng {order.OrderCode} đã được đánh dấu hoàn tất.",
                "Info"),
            _ => ("Cập nhật đơn hàng", $"Đơn hàng {order.OrderCode} đã được cập nhật.", "Info")
        };

        await _notifService.SendToUserAsync(
            order.UserId,
            title: notiTitle,
            message: notiMsg,
            type: notiType,
            redirectUrl: $"/dashboard/orders/{order.Id}");

        var user = await _userRepo.GetByIdAsync(order.UserId);
        if (user is not null && request.NewStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase))
        {
            _ = _emailSender.SendAsync(new SendEmailMessage
            {
                To = user.Email,
                Subject = $"Xác nhận thanh toán đơn hàng {order.OrderCode}",
                Body = EmailTemplates.OrderConfirm(
                    user.FullName,
                    order.OrderCode,
                    order.FinalAmount,
                    DateTime.UtcNow,
                    order.Items.Select(i => i.Template?.Name ?? i.TemplateName ?? "").ToList()),
                Template = "OrderConfirm"
            });
        }

        return ApiResponse<object>.Ok(
            new { orderId = order.Id, oldStatus, newStatus = request.NewStatus },
            $"Đã cập nhật đơn hàng sang trạng thái {request.NewStatus}");
    }

    // ─────────────────────────────────────────────────────────────────────────
    private static string? ValidateTransition(string current, string next)
    {
        // Cancelled không thể chuyển sang gì
        if (current.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            return "Không thể cập nhật đơn đã bị hủy";

        // Paid → chỉ được chuyển sang Completed
        if (current.Equals("Paid", StringComparison.OrdinalIgnoreCase)
            && !next.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            return "Đơn đã Paid chỉ có thể chuyển sang Completed";

        // Completed → không đổi được nữa
        if (current.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            return "Đơn đã hoàn tất, không thể thay đổi trạng thái";

        return null;
    }
}
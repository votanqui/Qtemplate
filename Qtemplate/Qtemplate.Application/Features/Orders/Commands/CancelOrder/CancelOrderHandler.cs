using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Domain.Messages;

namespace Qtemplate.Application.Features.Orders.Commands.CancelOrder;

public class CancelOrderHandler : IRequestHandler<CancelOrderCommand, ApiResponse<object>>
{
    private readonly IOrderRepository _orderRepo;
    private readonly ICouponRepository _couponRepo;
    private readonly IUserRepository _userRepo;
    private readonly IEmailSender _emailSender;
    private readonly INotificationService _notifService;
    public CancelOrderHandler(
        IOrderRepository orderRepo,
        ICouponRepository couponRepo,
        IUserRepository userRepo,
        IEmailSender emailSender,
        INotificationService notifService)
    {
        _orderRepo = orderRepo;
        _couponRepo = couponRepo;
        _userRepo = userRepo;
        _emailSender = emailSender;
        _notifService = notifService;
    }

    public async Task<ApiResponse<object>> Handle(
        CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepo.GetByIdAsync(request.OrderId);
        if (order is null)
            return ApiResponse<object>.Fail("Không tìm thấy đơn hàng");

        if (!request.IsAdmin && order.UserId != request.UserId)
            return ApiResponse<object>.Fail("Không có quyền hủy đơn này");

        if (order.Status == "Paid")
            return ApiResponse<object>.Fail("Không thể hủy đơn đã thanh toán");

        if (order.Status == "Cancelled")
            return ApiResponse<object>.Fail("Đơn hàng đã bị hủy rồi");

        order.Status = "Cancelled";
        order.CancelReason = request.CancelReason;
        order.CancelledAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepo.UpdateAsync(order);

        // Hoàn lại UsedCount coupon
        if (!string.IsNullOrEmpty(order.CouponCode))
        {
            var coupon = await _couponRepo.GetByCodeAsync(order.CouponCode);
            if (coupon is not null)
            {
                coupon.UsedCount = Math.Max(0, coupon.UsedCount - 1);
                await _couponRepo.UpdateAsync(coupon);
            }
        }
        await _notifService.SendToUserAsync(
                order.UserId,
                "Đơn hàng đã bị hủy",
                $"Đơn hàng {order.OrderCode} đã bị hủy. Lý do: {request.CancelReason}",
                type: "Warning",
                redirectUrl: $"/dashboard/orders/{order.Id}");
        // Gửi email thông báo hủy đơn
        var user = await _userRepo.GetByIdAsync(order.UserId);
        if (user is not null)
        {
            _ = _emailSender.SendAsync(new SendEmailMessage
            {
                To = user.Email,
                Subject = $"Đơn hàng {order.OrderCode} đã bị hủy",
                Body = EmailTemplates.OrderCancelled(
                    user.FullName,
                    order.OrderCode,
                    order.FinalAmount,
                    request.CancelReason),
                Template = "OrderCancelled"
            });
        }

        return ApiResponse<object>.Ok(null!, "Đã hủy đơn hàng");
    }
}
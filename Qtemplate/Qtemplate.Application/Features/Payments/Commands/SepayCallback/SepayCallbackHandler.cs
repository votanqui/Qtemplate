using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Application.Services;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Domain.Messages;
using System.Text.RegularExpressions;

namespace Qtemplate.Application.Features.Payments.Commands.SepayCallback;

public class SepayCallbackHandler : IRequestHandler<SepayCallbackCommand, ApiResponse<object>>
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly ITemplateRepository _templateRepo;
    private readonly IUserRepository _userRepo;
    private readonly IEmailSender _emailSender;

    public SepayCallbackHandler(
        IPaymentRepository paymentRepo,
        IOrderRepository orderRepo,
        ITemplateRepository templateRepo,
        IUserRepository userRepo,
        IEmailSender emailSender)
    {
        _paymentRepo = paymentRepo;
        _orderRepo = orderRepo;
        _templateRepo = templateRepo;
        _userRepo = userRepo;
        _emailSender = emailSender;
    }

    public async Task<ApiResponse<object>> Handle(
        SepayCallbackCommand request, CancellationToken cancellationToken)
    {
        // 1. Chỉ xử lý tiền vào
        if (!string.IsNullOrEmpty(request.TransferType)
            && request.TransferType.ToLower() != "in")
            return ApiResponse<object>.Ok(null!, "Bỏ qua giao dịch chuyển ra");

        // 2. Extract OrderCode từ content bằng regex
        //    Content có thể là: "QT-20260226-0F64BB27"
        //    hoặc: "chuyen tien QT-20260226-0F64BB27 cam on"
        var orderCode = ExtractOrderCode(request.Content);
        if (string.IsNullOrEmpty(orderCode))
            return ApiResponse<object>.Ok(null!,
                $"Nội dung '{request.Content}' không chứa mã đơn hàng hợp lệ");

        // 3. Tìm payment theo TransferContent = OrderCode
        var payment = await _paymentRepo.GetByTransferContentAsync(orderCode);
        if (payment is null)
            return ApiResponse<object>.Ok(null!,
                $"Không tìm thấy payment cho đơn: {orderCode}");

        // 4. Kiểm tra duplicate
        if (payment.Status == "Paid")
            return ApiResponse<object>.Ok(null!, "Giao dịch đã được xử lý trước đó");

        var order = payment.Order;

        if (order.Status == "Cancelled")
            return ApiResponse<object>.Ok(null!, "Đơn hàng đã bị hủy");

        // 5. Kiểm tra duplicate SepayId
        if (!string.IsNullOrEmpty(payment.SepayCode)
            && payment.SepayCode == request.SepayId)
            return ApiResponse<object>.Ok(null!, "SepayId đã được xử lý");

        // 6. Kiểm tra số tiền — cho phép nạp thừa
        if (request.TransferAmount < order.FinalAmount)
        {
            payment.Status = "Failed";
            payment.FailReason = $"Số tiền không đủ: nhận {request.TransferAmount:N0}đ, cần {order.FinalAmount:N0}đ";
            payment.RawCallback = request.RawCallback;
            await _paymentRepo.UpdateAsync(payment);
            return ApiResponse<object>.Ok(null!, payment.FailReason);
        }

        // 7. Cập nhật Payment
        payment.Status = "Paid";
        payment.SepayCode = request.SepayId;
        payment.BankCode = request.Gateway ?? payment.BankCode;
        payment.Amount = request.TransferAmount;
        payment.PaidAt = ParseDate(request.TransactionDate);
        payment.RawCallback = request.RawCallback;
        await _paymentRepo.UpdateAsync(payment);

        // 8. Cập nhật Order
        order.Status = "Paid";
        order.UpdatedAt = DateTime.UtcNow;
        await _orderRepo.UpdateAsync(order);

        // 9. Tăng SalesCount
        foreach (var item in order.Items)
        {
            var template = await _templateRepo.GetByIdAsync(item.TemplateId);
            if (template is not null)
            {
                template.SalesCount++;
                await _templateRepo.UpdateAsync(template);
            }
        }
        var user = await _userRepo.GetByIdAsync(order.UserId);
        if (user is not null)
        {
            _ = _emailSender.SendAsync(new SendEmailMessage
            {
                To = user.Email,
                Subject = $"Xác nhận thanh toán đơn hàng {order.OrderCode}",
                Body = EmailTemplates.OrderConfirm(
                    user.FullName,
                    order.OrderCode,
                    order.FinalAmount,
                    payment.PaidAt ?? DateTime.UtcNow,
                    order.Items.Select(i => i.Template?.Name ?? "").ToList()),
                Template = "OrderConfirm"
            });
        }
        return ApiResponse<object>.Ok(new
        {
            orderCode = order.OrderCode,
            amount = request.TransferAmount,
            gateway = request.Gateway,
            sepayId = request.SepayId,
            paidAt = payment.PaidAt
        }, $"Thanh toán thành công đơn {orderCode}");
    }

    /// <summary>
    /// Extract QT-YYYYMMDD-XXXXXXXX từ content bất kỳ
    /// VD: "chuyen tien QT-20260226-0F64BB27 xin cam on" → "QT-20260226-0F64BB27"
    /// </summary>
    private static string? ExtractOrderCode(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;
        var match = Regex.Match(content.ToUpper(), @"QT-\d{8}-[A-Z0-9]{8}");
        return match.Success ? match.Value : null;
    }

    private static DateTime ParseDate(string? dateStr)
    {
        if (string.IsNullOrEmpty(dateStr)) return DateTime.UtcNow;
        return DateTime.TryParse(dateStr, out var dt) ? dt : DateTime.UtcNow;
    }
}
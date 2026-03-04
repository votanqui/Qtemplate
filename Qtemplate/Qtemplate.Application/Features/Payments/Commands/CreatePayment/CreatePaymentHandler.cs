using MediatR;
using Qtemplate.Application.Constants;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Order;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Payments.Commands.CreatePayment;

public class CreatePaymentHandler : IRequestHandler<CreatePaymentCommand, ApiResponse<CreatePaymentResultDto>>
{
    private readonly IOrderRepository _orderRepo;
    private readonly IPaymentRepository _paymentRepo;
    private readonly ISettingRepository _settingRepo;

    public CreatePaymentHandler(
        IOrderRepository orderRepo,
        IPaymentRepository paymentRepo,
        ISettingRepository settingRepo)
    {
        _orderRepo = orderRepo;
        _paymentRepo = paymentRepo;
        _settingRepo = settingRepo;
    }

    public async Task<ApiResponse<CreatePaymentResultDto>> Handle(
        CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepo.GetByIdAsync(request.OrderId);
        if (order is null)
            return ApiResponse<CreatePaymentResultDto>.Fail("Không tìm thấy đơn hàng");

        if (order.UserId != request.UserId)
            return ApiResponse<CreatePaymentResultDto>.Fail("Không có quyền thanh toán đơn này");

        if (order.Status != "Pending")
            return ApiResponse<CreatePaymentResultDto>.Fail($"Đơn hàng đang ở trạng thái {order.Status}");

        // Lấy config từ DB Settings
        var settings = await _settingRepo.GetGroupAsync("Payment");
        var bankCode = settings.GetValueOrDefault(SettingKeys.SepayBankCode, "MB");
        var accountNumber = settings.GetValueOrDefault(SettingKeys.SepayAccountNumber, "");
        var qrBaseUrl = settings.GetValueOrDefault(SettingKeys.SepayQrBaseUrl, "https://qr.sepay.vn/img");

        // Kiểm tra đã có payment pending → trả lại luôn
        var existing = await _paymentRepo.GetByOrderIdAsync(order.Id);
        if (existing is not null && existing.Status == "Pending")
            return BuildResult(existing, order, bankCode, accountNumber, qrBaseUrl);

        // Nội dung CK = OrderCode (user ghi vào khi CK)
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            TransferContent = order.OrderCode,   // ← "QT-20260226-0F64BB27"
            BankCode = bankCode,
            AccountNumber = accountNumber,
            Amount = order.FinalAmount,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _paymentRepo.AddAsync(payment);
        return BuildResult(payment, order, bankCode, accountNumber, qrBaseUrl);
    }

    private static ApiResponse<CreatePaymentResultDto> BuildResult(
        Payment payment, Order order,
        string bankCode, string accountNumber, string qrBaseUrl)
    {
        // QR URL từ settings
        var qrUrl = $"{qrBaseUrl}?bank={bankCode}&acc={accountNumber}" +
                    $"&template=compact&amount={payment.Amount}" +
                    $"&des={Uri.EscapeDataString(payment.TransferContent ?? order.OrderCode)}";

        return ApiResponse<CreatePaymentResultDto>.Ok(new CreatePaymentResultDto
        {
            PaymentId = payment.Id,
            OrderCode = order.OrderCode,
            Amount = payment.Amount,
            TransferContent = payment.TransferContent ?? order.OrderCode,
            BankCode = bankCode,
            AccountNumber = accountNumber,
            QrUrl = qrUrl
        }, "Vui lòng chuyển khoản đúng nội dung và số tiền bên dưới");
    }
}
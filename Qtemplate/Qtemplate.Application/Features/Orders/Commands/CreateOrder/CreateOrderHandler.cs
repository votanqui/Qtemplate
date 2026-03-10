using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Order;
using Qtemplate.Application.Mappers;
using Qtemplate.Application.Services;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Domain.Messages;

namespace Qtemplate.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, ApiResponse<OrderDto>>
{
    private readonly IOrderRepository _orderRepo;
    private readonly ITemplateRepository _templateRepo;
    private readonly ICouponRepository _couponRepo;
    private readonly IUserRepository _userRepo;
    private readonly IEmailSender _emailSender;
    private readonly INotificationService _notifService;
    private readonly IAffiliateRepository _affiliateRepo;
    public CreateOrderHandler(
        IOrderRepository orderRepo,
        ITemplateRepository templateRepo,
        ICouponRepository couponRepo,
        IUserRepository userRepo,
        IEmailSender emailSender,
        INotificationService notifService,
        IAffiliateRepository affiliateRepo)
    {
        _orderRepo = orderRepo;
        _templateRepo = templateRepo;
        _couponRepo = couponRepo;
        _userRepo = userRepo;
        _emailSender = emailSender;
        _notifService = notifService;
        _affiliateRepo = affiliateRepo;
    }

    public async Task<ApiResponse<OrderDto>> Handle(
        CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (!request.TemplateIds.Any())
            return ApiResponse<OrderDto>.Fail("Vui lòng chọn ít nhất 1 template");

        // Load & validate templates
        var templates = new List<Domain.Entities.Template>();
        foreach (var tid in request.TemplateIds.Distinct())
        {
            var t = await _templateRepo.GetByIdAsync(tid);
            if (t is null || t.Status != "Published")
                return ApiResponse<OrderDto>.Fail("Template không tồn tại hoặc chưa được publish");

            if (!t.IsFree && await _orderRepo.HasPurchasedAsync(request.UserId, tid))
                return ApiResponse<OrderDto>.Fail($"Bạn đã mua template '{t.Name}' rồi");

            templates.Add(t);
        }

        // Tính tổng
        decimal totalAmount = templates.Sum(t => t.SalePrice ?? t.Price);

        // Áp coupon
        decimal discountAmount = 0;
        Coupon? coupon = null;

        if (!string.IsNullOrEmpty(request.CouponCode))
        {
            coupon = await _couponRepo.GetByCodeAsync(request.CouponCode);
            var (isValid, error) = ValidateCoupon(coupon, totalAmount);
            if (!isValid) return ApiResponse<OrderDto>.Fail(error!);
            discountAmount = CalculateDiscount(coupon!, totalAmount);
        }

        decimal finalAmount = Math.Max(0, totalAmount - discountAmount);
        var orderId = Guid.NewGuid();
        var orderCode = $"QT-{DateTime.UtcNow:yyyyMMdd}-{orderId.ToString()[..8].ToUpper()}";

        var order = new Order
        {
            Id = orderId,
            UserId = request.UserId,
            OrderCode = orderCode,
            TotalAmount = totalAmount,
            DiscountAmount = discountAmount,
            FinalAmount = finalAmount,
            CouponCode = request.CouponCode,
            AffiliateCode = request.AffiliateCode,
            Status = finalAmount == 0 ? "Paid" : "Pending",
            Note = request.Note,
            CreatedAt = DateTime.UtcNow,
            Items = templates.Select(t => new OrderItem
            {
                OrderId = orderId,
                TemplateId = t.Id,
                TemplateName = t.Name,
                OriginalPrice = t.Price,
                Price = t.SalePrice ?? t.Price
            }).ToList()
        };

        await _orderRepo.AddAsync(order);

        // Tăng UsedCount coupon
        if (coupon is not null)
        {
            coupon.UsedCount++;
            await _couponRepo.UpdateAsync(coupon);
        }

        // Nếu free → tăng SalesCount + gửi email confirm luôn
        if (finalAmount == 0)
        {
            foreach (var t in templates)
            {
                t.SalesCount++;
                await _templateRepo.UpdateAsync(t);
            }
            await _notifService.SendToUserAsync(
                   request.UserId,
                   "Nhận template thành công 🎁",
                   $"Bạn đã nhận {templates.Count} template miễn phí.",
                   "Success",
                   "/dashboard/downloads"
   );
            await CreateAffiliateTransactionAsync(order, 0);
            var user = await _userRepo.GetByIdAsync(request.UserId);
            if (user is not null)
            {
                _ = _emailSender.SendAsync(new SendEmailMessage
                {
                    To = user.Email,
                    Subject = $"Xác nhận đơn hàng {orderCode}",
                    Body = EmailTemplates.OrderConfirm(
                        user.FullName,
                        orderCode,
                        0,
                        DateTime.UtcNow,
                        templates.Select(t => t.Name).ToList()),
                    Template = "OrderConfirm"
                });
            }
        }

        return ApiResponse<OrderDto>.Ok(
            OrderMapper.ToDto(order),
            finalAmount == 0
                ? "Đặt hàng thành công"
                : "Tạo đơn hàng thành công, vui lòng thanh toán");
    }

    private (bool IsValid, string? Error) ValidateCoupon(Coupon? coupon, decimal total)
    {
        if (coupon is null) return (false, "Mã giảm giá không tồn tại");
        if (!coupon.IsActive) return (false, "Mã giảm giá không còn hiệu lực");
        if (coupon.StartAt.HasValue && DateTime.UtcNow < coupon.StartAt) return (false, "Mã chưa có hiệu lực");
        if (coupon.ExpiredAt.HasValue && DateTime.UtcNow > coupon.ExpiredAt) return (false, "Mã đã hết hạn");
        if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit) return (false, "Mã đã hết lượt dùng");
        if (coupon.MinOrderAmount.HasValue && total < coupon.MinOrderAmount)
            return (false, $"Đơn hàng tối thiểu {coupon.MinOrderAmount:N0}đ");
        return (true, null);
    }

    private decimal CalculateDiscount(Coupon coupon, decimal total)
    {
        var discount = coupon.Type == "Percent"
            ? total * coupon.Value / 100
            : coupon.Value;

        if (coupon.MaxDiscountAmount.HasValue)
            discount = Math.Min(discount, coupon.MaxDiscountAmount.Value);

        return Math.Min(discount, total);
    }
    private async Task CreateAffiliateTransactionAsync(Order order, decimal paidAmount)
    {
        if (string.IsNullOrEmpty(order.AffiliateCode)) return;

        var affiliate = await _affiliateRepo.GetByCodeAsync(order.AffiliateCode);
        if (affiliate is null || !affiliate.IsActive) return;

        // Tránh duplicate
        var existing = await _affiliateRepo.GetTransactionsByAffiliateIdAsync(affiliate.Id);
        if (existing.Any(t => t.OrderId == order.Id)) return;

        var commission = Math.Round(paidAmount * affiliate.CommissionRate / 100, 2);

        await _affiliateRepo.AddTransactionAsync(new AffiliateTransaction
        {
            AffiliateId = affiliate.Id,
            OrderId = order.Id,
            OrderAmount = paidAmount,
            Commission = commission,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        });

        affiliate.TotalEarned += commission;
        affiliate.PendingAmount += commission;
        await _affiliateRepo.UpdateAsync(affiliate);
    }
}
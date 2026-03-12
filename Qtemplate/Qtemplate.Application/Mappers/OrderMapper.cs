using Qtemplate.Application.DTOs.Order;
using Qtemplate.Domain.Entities;

namespace Qtemplate.Application.Mappers;

public static class OrderMapper
{
    public static OrderDto ToDto(Order o) => new()
    {
        Id = o.Id,
        OrderCode = o.OrderCode,
        UserId = o.UserId,
        UserName = o.User?.FullName,
        UserEmail = o.User?.Email,
        TotalAmount = o.TotalAmount,
        DiscountAmount = o.DiscountAmount,
        FinalAmount = o.FinalAmount,
        CouponCode = o.CouponCode,
        Status = o.Status,
        Note = o.Note,
        CancelReason = o.CancelReason,
        CreatedAt = o.CreatedAt,
        // Payment — chỉ có khi GetByIdWithDetailsAsync
        PaymentStatus = o.Payment?.Status,
        BankCode = o.Payment?.BankCode,
        SepayCode = o.Payment?.SepayCode,
        TransferContent = o.Payment?.TransferContent,
        PaymentAmount = o.Payment?.Amount,
        PaidAt = o.Payment?.PaidAt,
        FailReason = o.Payment?.FailReason,
        Items = o.Items?.Select(ToItemDto).ToList() ?? new()
    };

    public static OrderItemDto ToItemDto(OrderItem i) => new()
    {
        Id = i.Id,
        TemplateId = i.TemplateId,
        TemplateName = i.TemplateName,          // snapshot lúc mua
        ThumbnailUrl = i.Template?.ThumbnailUrl, // null nếu không include
        TemplateSlug = i.Template?.Slug,
        OriginalPrice = i.OriginalPrice,
        Price = i.Price
    };
}
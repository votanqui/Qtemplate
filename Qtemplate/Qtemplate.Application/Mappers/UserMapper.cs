using Qtemplate.Application.DTOs.Order;
using Qtemplate.Application.DTOs.User;
using Qtemplate.Application.DTOs.Wishlist;
using Qtemplate.Domain.Entities;

namespace Qtemplate.Application.Mappers;

public static class UserMapper
{
    // ── Profile (user tự xem) ─────────────────────────────────────────────
    public static UserProfileDto ToProfileDto(User u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email,
        PhoneNumber = u.PhoneNumber,
        AvatarUrl = u.AvatarUrl,
        Role = u.Role,
        IsEmailVerified = u.IsEmailVerified,
        LastLoginAt = u.LastLoginAt,
        CreatedAt = u.CreatedAt
    };

    // ── UserInfo (trong AuthResponse) ─────────────────────────────────────
    public static UserInfoDto ToInfoDto(User u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email,
        Role = u.Role,
        AvatarUrl = u.AvatarUrl
    };

    // ── Admin user list ───────────────────────────────────────────────────
    public static AdminUserDto ToAdminDto(User u,
        int totalOrders = 0, decimal totalSpent = 0) => new()
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            AvatarUrl = u.AvatarUrl,
            Role = u.Role,
            IsActive = u.IsActive,
            IsEmailVerified = u.IsEmailVerified,
            LastLoginAt = u.LastLoginAt,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt,
            TotalOrders = totalOrders,
            TotalSpent = totalSpent
        };

    // ── Purchase history item ─────────────────────────────────────────────
    public static PurchaseHistoryItemDto ToPurchaseHistoryDto(Order o) => new()
    {
        OrderId = o.Id,
        OrderCode = o.OrderCode,
        TotalAmount = o.TotalAmount,
        DiscountAmount = o.DiscountAmount,
        FinalAmount = o.FinalAmount,
        CouponCode = o.CouponCode,
        Status = o.Status,
        CancelReason = o.CancelReason,
        PaymentStatus = o.Payment?.Status,
        BankCode = o.Payment?.BankCode,
        PaidAt = o.Payment?.PaidAt,
        CreatedAt = o.CreatedAt,
        Items = o.Items?.Select(i => ToPurchaseItemDto(i, o.Status)).ToList() ?? new()
    };

    public static OrderItemDto ToPurchaseItemDto(OrderItem i, string orderStatus) => new()
    {
        Id = i.Id,
        TemplateId = i.TemplateId,
        TemplateName = i.TemplateName,
        OriginalPrice = i.OriginalPrice,
        Price = i.Price,
        ThumbnailUrl = i.Template?.ThumbnailUrl,
        TemplateSlug = i.Template?.Slug,
        // Chỉ trả download URL khi đã paid và có slug
        DownloadUrl = (orderStatus is "Paid" or "Completed") && i.Template?.Slug != null
            ? $"/api/templates/{i.Template.Slug}/download"
            : null
    };

    // ── Wishlist item ─────────────────────────────────────────────────────
    public static WishlistItemDto ToWishlistDto(Wishlist w) => new()
    {
        Id = w.Id,
        TemplateId = w.TemplateId,
        TemplateName = w.Template.Name,
        TemplateSlug = w.Template.Slug,
        ThumbnailUrl = w.Template.ThumbnailUrl,
        Price = w.Template.Price,
        SalePrice = w.Template.SalePrice,
        IsFree = w.Template.IsFree,
        Status = w.Template.Status,
        CreatedAt = w.CreatedAt
    };

    // ── Admin wishlist item ───────────────────────────────────────────────
    public static AdminWishlistItemDto ToAdminWishlistDto(Wishlist w) => new()
    {
        Id = w.Id,
        UserId = w.UserId,
        UserEmail = w.User.Email,
        TemplateId = w.TemplateId,
        TemplateName = w.Template.Name,
        CreatedAt = w.CreatedAt
    };
}
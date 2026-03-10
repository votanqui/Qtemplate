using Qtemplate.Application.DTOs.Order;
namespace Qtemplate.Application.DTOs.User;

public class UpdateProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class UpdateAvatarDto
{
    public string AvatarUrl { get; set; } = string.Empty;
}

public class DeleteAccountDto
{
    public string Password { get; set; } = string.Empty;
}

// ── Purchase / Order ───────────────────────────────
public class PurchaseHistoryItemDto
{
    public Guid OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string? CouponCode { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CancelReason { get; set; }
    public string? PaymentStatus { get; set; }
    public string? BankCode { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}


// ── Download ───────────────────────────────────────
public class DownloadHistoryItemDto
{
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? Slug { get; set; }
    public int DownloadCount { get; set; }
    public DateTime? LastDownloadAt { get; set; }
}

// ── Notification ───────────────────────────────────
public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? RedirectUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ── Wishlist ───────────────────────────────────────
public class ToggleWishlistResultDto
{
    public bool IsWishlisted { get; set; }
}

// ── Pagination (giữ lại tương thích, dùng PaginatedResult<T> là chuẩn) ──
public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
public class AdminUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    // Stats
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
}
public class ChangeUserStatusDto
{
    public bool IsActive { get; set; }
    public string? Reason { get; set; }
}

public class ChangeUserRoleDto
{
    public string Role { get; set; } = string.Empty;
}
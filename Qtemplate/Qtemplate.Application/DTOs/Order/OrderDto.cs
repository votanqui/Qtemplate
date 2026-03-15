namespace Qtemplate.Application.DTOs.Order;

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    // User info
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string? CouponCode { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string? CancelReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    // Payment
    public string? PaymentStatus { get; set; }
    public string? BankCode { get; set; }
    public string? SepayCode { get; set; }
    public string? TransferContent { get; set; }
    public decimal? PaymentAmount { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? FailReason { get; set; }
}

public class OrderItemDto
{
    public int Id { get; set; }
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? TemplateSlug { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal Price { get; set; }
    public string? DownloadUrl { get; set; }  // ← thêm
}

public class CreateOrderDto
{
    public List<Guid> TemplateIds { get; set; } = new();
    public string? CouponCode { get; set; }
    public string? AffiliateCode { get; set; }
    public string? Note { get; set; }
}

public class ApplyCouponDto
{
    public string CouponCode { get; set; } = string.Empty;
    public List<Guid> TemplateIds { get; set; } = new();
}

public class ApplyCouponResultDto
{
    public string CouponCode { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
}

public class CreatePaymentResultDto
{
    public Guid PaymentId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransferContent { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string QrUrl { get; set; } = string.Empty;
}
public class CancelReasonDto
{
    public string? Reason { get; set; }
}
public class UpdateOrderStatusDto
{
    public string NewStatus { get; set; } = string.Empty;
    public string? Note { get; set; }
}
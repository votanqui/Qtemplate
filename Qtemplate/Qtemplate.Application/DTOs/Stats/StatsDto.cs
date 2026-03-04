namespace Qtemplate.Application.DTOs.Stats;

public class DashboardStatsDto
{
    public OrderStatsDto Orders { get; set; } = new();
    public PaymentStatsDto Payments { get; set; } = new();
    public CouponStatsDto Coupons { get; set; } = new();
}

// ── Order ─────────────────────────────────────────
public class OrderStatsDto
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int PaidOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalDiscount { get; set; }

    public List<RevenueByPeriodDto> RevenueByDay { get; set; } = new();
    public List<RevenueByPeriodDto> RevenueByMonth { get; set; } = new();
    public List<RevenueByPeriodDto> RevenueByYear { get; set; } = new();
    public List<TopTemplateDto> TopTemplates { get; set; } = new();
    public List<TopUserDto> TopUsers { get; set; } = new();
}

public class RevenueByPeriodDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
}

public class TopTemplateDto
{
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public int SalesCount { get; set; }
    public decimal Revenue { get; set; }
}

public class TopUserDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
}

// ── Payment ───────────────────────────────────────
public class PaymentStatsDto
{
    public int TotalTransactions { get; set; }
    public int SuccessTransactions { get; set; }
    public int FailedTransactions { get; set; }
    public int PendingTransactions { get; set; }
    public decimal TotalPaid { get; set; }

    public List<RevenueByBankDto> ByBank { get; set; } = new();
}

public class RevenueByBankDto
{
    public string BankCode { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
}

// ── Coupon ────────────────────────────────────────
public class CouponStatsDto
{
    public int TotalCoupons { get; set; }
    public int ActiveCoupons { get; set; }
    public int ExpiredCoupons { get; set; }
    public decimal TotalDiscounted { get; set; }

    public List<TopCouponDto> TopCoupons { get; set; } = new();
}

public class TopCouponDto
{
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public int UsedCount { get; set; }
    public decimal TotalDiscount { get; set; }
}
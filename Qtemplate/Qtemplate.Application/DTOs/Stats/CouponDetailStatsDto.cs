namespace Qtemplate.Application.DTOs.Stats;

public class CouponDetailStatsDto
{
    // Tổng quan
    public int TotalCoupons { get; set; }
    public int ActiveCoupons { get; set; }
    public int ExpiredCoupons { get; set; }
    public decimal TotalDiscounted { get; set; }

    // Tỉ lệ đơn dùng coupon
    public int OrdersWithCoupon { get; set; }
    public int OrdersWithoutCoupon { get; set; }
    public decimal CouponUsageRate { get; set; }  // %

    // Tiết kiệm trung bình mỗi đơn dùng coupon
    public decimal AverageDiscount { get; set; }

    // Top coupon
    public List<TopCouponDto> TopCoupons { get; set; } = new();

    // Sắp hết hạn (7 ngày tới)
    public List<ExpiringSoonDto> ExpiringSoon { get; set; } = new();

    // Sắp hết lượt dùng (còn <= 5 lượt)
    public List<LowUsageDto> LowUsage { get; set; } = new();
}

public class ExpiringSoonDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime ExpiredAt { get; set; }
    public int DaysLeft { get; set; }
    public int UsedCount { get; set; }
    public int? UsageLimit { get; set; }
}

public class LowUsageDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int UsedCount { get; set; }
    public int UsageLimit { get; set; }
    public int RemainingUse { get; set; }
}
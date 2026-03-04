namespace Qtemplate.Domain.Entities;

public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = "Percent";           // Percent / Fixed
    public decimal Value { get; set; }                      // 20 = 20% hoặc 20k
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }                    // Tổng số lần dùng
    public int UsedCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime? StartAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
namespace Qtemplate.Domain.Entities;

public class Affiliate
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string AffiliateCode { get; set; } = string.Empty; // Code giới thiệu
    public decimal CommissionRate { get; set; } = 10;          // % hoa hồng
    public decimal TotalEarned { get; set; } = 0;
    public decimal PendingAmount { get; set; } = 0;
    public decimal PaidAmount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<AffiliateTransaction> Transactions { get; set; } = new List<AffiliateTransaction>();
}
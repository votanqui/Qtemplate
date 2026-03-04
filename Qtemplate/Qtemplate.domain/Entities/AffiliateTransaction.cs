namespace Qtemplate.Domain.Entities;

public class AffiliateTransaction
{
    public int Id { get; set; }
    public int AffiliateId { get; set; }
    public Guid OrderId { get; set; }
    public decimal OrderAmount { get; set; }
    public decimal Commission { get; set; }
    public string Status { get; set; } = "Pending";          // Pending / Approved / Paid
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Affiliate Affiliate { get; set; } = null!;
    public Order Order { get; set; } = null!;
}
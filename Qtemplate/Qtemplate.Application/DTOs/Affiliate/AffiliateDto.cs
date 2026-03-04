namespace Qtemplate.Application.DTOs.Affiliate;

public class AffiliateDto
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string AffiliateCode { get; set; } = string.Empty;
    public decimal CommissionRate { get; set; }
    public decimal TotalEarned { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<AffiliateTransactionDto> Transactions { get; set; } = new();
}

public class AffiliateTransactionDto
{
    public int Id { get; set; }
    public Guid OrderId { get; set; }
    public string? OrderCode { get; set; }
    public decimal OrderAmount { get; set; }
    public decimal Commission { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ApproveAffiliateDto
{
    public bool IsActive { get; set; } = true;
    public decimal CommissionRate { get; set; } = 10;
}
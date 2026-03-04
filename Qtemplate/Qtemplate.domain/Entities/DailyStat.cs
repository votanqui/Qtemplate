namespace Qtemplate.Domain.Entities;

public class DailyStat
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int TotalOrders { get; set; } = 0;
    public int PaidOrders { get; set; } = 0;
    public int CancelledOrders { get; set; } = 0;
    public decimal TotalRevenue { get; set; } = 0;
    public int NewUsers { get; set; } = 0;
    public int PageViews { get; set; } = 0;
    public int UniqueVisitors { get; set; } = 0;
    public int NewReviews { get; set; } = 0;
    public int NewTickets { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
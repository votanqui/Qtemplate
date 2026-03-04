namespace Qtemplate.Application.DTOs.Stats;

public class PaymentDetailStatsDto
{
    // Tổng quan
    public int TotalTransactions { get; set; }
    public int SuccessTransactions { get; set; }
    public int FailedTransactions { get; set; }
    public int PendingTransactions { get; set; }
    public decimal TotalPaid { get; set; }

    // Tỉ lệ
    public decimal SuccessRate { get; set; }  // %
    public decimal FailureRate { get; set; }  // %

    // Trung bình
    public decimal AverageTransactionValue { get; set; }

    // Theo ngân hàng
    public List<RevenueByBankDto> ByBank { get; set; } = new();

    // Theo giờ hôm nay
    public List<HourlyStatDto> HourlyToday { get; set; } = new();

    // Giao dịch thất bại gần đây
    public List<FailedTransactionDto> RecentFailed { get; set; } = new();
}

public class FailedTransactionDto
{
    public Guid PaymentId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? FailReason { get; set; }
    public string? BankCode { get; set; }
    public DateTime CreatedAt { get; set; }
}
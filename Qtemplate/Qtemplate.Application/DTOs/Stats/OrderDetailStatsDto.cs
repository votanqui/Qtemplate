namespace Qtemplate.Application.DTOs.Stats;

public class OrderDetailStatsDto
{
    // Tổng quan nhanh
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public int TodayOrders { get; set; }
    public int WeekOrders { get; set; }
    public int MonthOrders { get; set; }

    // Tỉ lệ
    public decimal CompletionRate { get; set; }  // % Paid
    public decimal CancellationRate { get; set; }  // % Cancelled

    // So sánh kỳ này vs kỳ trước (theo khoảng from-to)
    public decimal CurrentRevenue { get; set; }
    public decimal PreviousRevenue { get; set; }
    public decimal RevenueGrowth { get; set; }  // % tăng trưởng
    public int CurrentOrders { get; set; }
    public int PreviousOrders { get; set; }
    public decimal OrderGrowth { get; set; }  // % tăng trưởng

    // Biểu đồ theo giờ hôm nay
    public List<HourlyStatDto> HourlyToday { get; set; } = new();

    // Biểu đồ theo ngày trong khoảng
    public List<RevenueByPeriodDto> ByDay { get; set; } = new();

    // Số đơn theo trạng thái
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int PaidOrders { get; set; }
    public int CancelledOrders { get; set; }
}

public class HourlyStatDto
{
    public int Hour { get; set; }  // 0-23
    public int Orders { get; set; }
    public decimal Revenue { get; set; }
}
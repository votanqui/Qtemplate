using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.DTOs.Stats
{
    public class DailyStatDto
    {
        public DateTime Date { get; set; }
        public string DateLabel { get; set; } = string.Empty; // "15/03", "T3/2026"...
        public int TotalOrders { get; set; }
        public int PaidOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int NewUsers { get; set; }
        public int PageViews { get; set; }
        public int UniqueVisitors { get; set; }
        public int NewReviews { get; set; }
        public int NewTickets { get; set; }
    }
    public class DailyStatsResultDto
    {
        public List<DailyStatDto> Items { get; set; } = new();
        public string Period { get; set; } = string.Empty; // "daily" | "weekly" | "monthly"
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        // Tổng hợp nhanh cho summary cards
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int PaidOrders { get; set; }
        public int NewUsers { get; set; }
        public int PageViews { get; set; }
    }
}

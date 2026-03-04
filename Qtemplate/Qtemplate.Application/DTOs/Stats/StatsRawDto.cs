namespace Qtemplate.Application.DTOs.Stats;

public class OrderStatsRawDto
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Paid { get; set; }
    public int Cancelled { get; set; }
    public decimal Revenue { get; set; }
    public decimal Discount { get; set; }

    public List<PeriodStat> ByDay { get; set; } = new();
    public List<PeriodStat> ByMonth { get; set; } = new();
    public List<PeriodStat> ByYear { get; set; } = new();
    public List<TemplateStat> TopTemplates { get; set; } = new();
    public List<UserStat> TopUsers { get; set; } = new();
}

public class PeriodStat
{
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Count { get; set; }
}

public class TemplateStat
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Revenue { get; set; }
}

public class UserStat
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public int Count { get; set; }
    public decimal Spent { get; set; }
}

public class PaymentStatsRawDto
{
    public int Total { get; set; }
    public int Success { get; set; }
    public int Failed { get; set; }
    public int Pending { get; set; }
    public decimal TotalPaid { get; set; }

    public List<BankStat> ByBank { get; set; } = new();
}

public class BankStat
{
    public string BankCode { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

public class CouponStatsRawDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Expired { get; set; }
    public decimal TotalDiscounted { get; set; }

    public List<CouponStat> Top { get; set; } = new();
}

public class CouponStat
{
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public int UsedCount { get; set; }
    public decimal TotalDiscount { get; set; }
}
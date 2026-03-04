using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Stats;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Stats.Queries.GetPaymentDetailStats;

public class GetPaymentDetailStatsHandler
    : IRequestHandler<GetPaymentDetailStatsQuery, ApiResponse<PaymentDetailStatsDto>>
{
    private readonly IStatsRepository _stats;
    public GetPaymentDetailStatsHandler(IStatsRepository stats) => _stats = stats;

    public async Task<ApiResponse<PaymentDetailStatsDto>> Handle(
        GetPaymentDetailStatsQuery request, CancellationToken cancellationToken)
    {
        var to = (request.To ?? DateTime.UtcNow).Date.AddDays(1).AddSeconds(-1);
        var from = (request.From ?? to.AddDays(-29)).Date;
        var today = DateTime.UtcNow.Date;

        var payments = await _stats.GetPaymentsInRangeAsync(from, to, includeOrder: true);
        var todayPayments = await _stats.GetPaymentsInRangeAsync(today, today.AddDays(1).AddSeconds(-1));

        var success = payments.Where(p => p.Status == "Paid").ToList();
        var failed = payments.Where(p => p.Status == "Failed").ToList();
        var total = payments.Count;

        return ApiResponse<PaymentDetailStatsDto>.Ok(new PaymentDetailStatsDto
        {
            TotalTransactions = total,
            SuccessTransactions = success.Count,
            FailedTransactions = failed.Count,
            PendingTransactions = payments.Count(p => p.Status == "Pending"),
            TotalPaid = success.Sum(p => p.Amount),
            SuccessRate = total == 0 ? 0 : Math.Round((decimal)success.Count / total * 100, 2),
            FailureRate = total == 0 ? 0 : Math.Round((decimal)failed.Count / total * 100, 2),
            AverageTransactionValue = success.Count == 0 ? 0
                : Math.Round(success.Sum(p => p.Amount) / success.Count, 0),
            ByBank = success
                .Where(p => !string.IsNullOrEmpty(p.BankCode))
                .GroupBy(p => p.BankCode!)
                .Select(g => new RevenueByBankDto
                {
                    BankCode = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(p => p.Amount)
                })
                .OrderByDescending(x => x.TotalAmount).ToList(),
            HourlyToday = Enumerable.Range(0, 24).Select(h => new HourlyStatDto
            {
                Hour = h,
                Orders = todayPayments.Count(p => p.CreatedAt.Hour == h),
                Revenue = todayPayments
                    .Where(p => p.CreatedAt.Hour == h && p.Status == "Paid")
                    .Sum(p => p.Amount)
            }).ToList(),
            RecentFailed = failed
                .OrderByDescending(p => p.CreatedAt)
                .Take(20)
                .Select(p => new FailedTransactionDto
                {
                    PaymentId = p.Id,
                    OrderCode = p.Order?.OrderCode ?? "",
                    Amount = p.Amount,
                    FailReason = p.FailReason,
                    BankCode = p.BankCode,
                    CreatedAt = p.CreatedAt
                }).ToList()
        });
    }
}
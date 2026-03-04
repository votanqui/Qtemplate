using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Admin;
using Qtemplate.Application.DTOs.Email;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Stats.Queries.GetSecurityStats;

public class GetSecurityStatsHandler
    : IRequestHandler<GetSecurityStatsQuery, ApiResponse<SecurityStatsDto>>
{
    private readonly IStatsRepository _stats;
    public GetSecurityStatsHandler(IStatsRepository stats) => _stats = stats;

    public async Task<ApiResponse<SecurityStatsDto>> Handle(
        GetSecurityStatsQuery request, CancellationToken cancellationToken)
    {
        var to = (request.To ?? DateTime.UtcNow).Date.AddDays(1).AddSeconds(-1);
        var from = (request.From ?? to.AddDays(-29)).Date;

        var ipList = await _stats.GetAllIpBlacklistAsync();
        var reqLogs = await _stats.GetRequestLogsInRangeAsync(from, to);
        var emailLogs = await _stats.GetAllEmailLogsAsync();

        // ── IpBlacklist stats ─────────────────────────────────────────────────
        var ipStats = new IpBlacklistStatsDto
        {
            Total = ipList.Count,
            Active = ipList.Count(x => x.IsActive),
            Inactive = ipList.Count(x => !x.IsActive),
            Manual = ipList.Count(x => x.Type == "Manual"),
            Auto = ipList.Count(x => x.Type == "Auto"),
            Permanent = ipList.Count(x => x.ExpiredAt == null),
            Temporary = ipList.Count(x => x.ExpiredAt != null),
            RecentBlocked = ipList
                .OrderByDescending(x => x.BlockedAt)
                .Take(10)
                .Select(x => new RecentBlockedDto
                {
                    IpAddress = x.IpAddress,
                    Reason = x.Reason,
                    Type = x.Type,
                    BlockedAt = x.BlockedAt
                }).ToList()
        };

        // ── RequestLog stats ──────────────────────────────────────────────────
        var total = reqLogs.Count;
        var reqStats = new RequestLogStatsDto
        {
            TotalRequests = total,
            SuccessRequests = reqLogs.Count(r => r.StatusCode is >= 200 and < 300),
            ClientErrors = reqLogs.Count(r => r.StatusCode is >= 400 and < 500),
            ServerErrors = reqLogs.Count(r => r.StatusCode is >= 500),
            SuccessRate = total == 0 ? 0
                : Math.Round((double)reqLogs.Count(r => r.StatusCode < 400) / total * 100, 2),
            ErrorRate = total == 0 ? 0
                : Math.Round((double)reqLogs.Count(r => r.StatusCode >= 400) / total * 100, 2),
            AvgResponseTime = total == 0 ? 0
                : (long)reqLogs.Average(r => r.ResponseTimeMs),
            MaxResponseTime = total == 0 ? 0
                : reqLogs.Max(r => r.ResponseTimeMs),
            ByStatusCode = reqLogs
                .GroupBy(r => r.StatusCode)
                .Select(g => new StatusCodeStatDto
                {
                    StatusCode = g.Key,
                    Count = g.Count(),
                    Percentage = total == 0 ? 0
                        : Math.Round((double)g.Count() / total * 100, 2)
                })
                .OrderBy(x => x.StatusCode).ToList(),
            TopEndpoints = reqLogs
                .GroupBy(r => new { r.Endpoint, r.Method })
                .Select(g => new EndpointStatDto
                {
                    Endpoint = g.Key.Endpoint,
                    Method = g.Key.Method,
                    Count = g.Count(),
                    AvgResponseTime = (long)g.Average(r => r.ResponseTimeMs)
                })
                .OrderByDescending(x => x.Count)
                .Take(10).ToList(),
            TopIps = reqLogs
                .GroupBy(r => r.IpAddress)
                .Select(g => new IpRequestStatDto
                {
                    IpAddress = g.Key,
                    Count = g.Count(),
                    ErrorCount = g.Count(r => r.StatusCode >= 400)
                })
                .OrderByDescending(x => x.Count)
                .Take(10).ToList()
        };

        // ── EmailLog stats ────────────────────────────────────────────────────
        var emailTotal = emailLogs.Count;
        var emailStats = new EmailLogStatsDto
        {
            Total = emailTotal,
            Sent = emailLogs.Count(e => e.Status == "Sent"),
            Failed = emailLogs.Count(e => e.Status == "Failed"),
            Pending = emailLogs.Count(e => e.Status == "Pending"),
            SuccessRate = emailTotal == 0 ? 0
                : Math.Round((double)emailLogs.Count(e => e.Status == "Sent") / emailTotal * 100, 2),
            FailureRate = emailTotal == 0 ? 0
                : Math.Round((double)emailLogs.Count(e => e.Status == "Failed") / emailTotal * 100, 2),
            ByTemplate = emailLogs
                .GroupBy(e => e.Template)
                .Select(g => new EmailTemplateStatDto
                {
                    Template = g.Key,
                    Total = g.Count(),
                    Sent = g.Count(e => e.Status == "Sent"),
                    Failed = g.Count(e => e.Status == "Failed")
                })
                .OrderByDescending(x => x.Total).ToList(),
            RecentFailed = emailLogs
                .Where(e => e.Status == "Failed")
                .OrderByDescending(e => e.CreatedAt)
                .Take(10)
                .Select(e => new EmailLogDto
                {
                    Id = e.Id,
                    To = e.To,
                    Subject = e.Subject,
                    Template = e.Template,
                    Status = e.Status,
                    ErrorMessage = e.ErrorMessage,
                    RetryCount = e.RetryCount,
                    CreatedAt = e.CreatedAt
                }).ToList()
        };

        return ApiResponse<SecurityStatsDto>.Ok(new SecurityStatsDto
        {
            IpBlacklist = ipStats,
            RequestLogs = reqStats,
            EmailLogs = emailStats
        });
    }
}
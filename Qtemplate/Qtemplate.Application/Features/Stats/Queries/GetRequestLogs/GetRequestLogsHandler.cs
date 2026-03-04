using MediatR;
using Qtemplate.Application.DTOs.Request;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qtemplate.Application.DTOs.Admin;

namespace Qtemplate.Application.Features.Stats.Queries.GetRequestLogs
{
    public class GetRequestLogsHandler
      : IRequestHandler<GetRequestLogsQuery, ApiResponse<RequestLogStatsDto>>
    {
        private readonly IStatsRepository _stats;
        public GetRequestLogsHandler(IStatsRepository stats) => _stats = stats;

        public async Task<ApiResponse<RequestLogStatsDto>> Handle(
            GetRequestLogsQuery request, CancellationToken cancellationToken)
        {
            var to = (request.To ?? DateTime.UtcNow).Date.AddDays(1).AddSeconds(-1);
            var from = (request.From ?? to.AddDays(-29)).Date;

            var list = await _stats.GetRequestLogsInRangeAsync(from, to);
            var total = list.Count;

            return ApiResponse<RequestLogStatsDto>.Ok(new RequestLogStatsDto
            {
                TotalRequests = total,
                SuccessRequests = list.Count(r => r.StatusCode is >= 200 and < 300),
                ClientErrors = list.Count(r => r.StatusCode is >= 400 and < 500),
                ServerErrors = list.Count(r => r.StatusCode >= 500),
                SuccessRate = total == 0 ? 0
                    : Math.Round((double)list.Count(r => r.StatusCode < 400) / total * 100, 2),
                ErrorRate = total == 0 ? 0
                    : Math.Round((double)list.Count(r => r.StatusCode >= 400) / total * 100, 2),
                AvgResponseTime = total == 0 ? 0
                    : (long)list.Average(r => r.ResponseTimeMs),
                MaxResponseTime = total == 0 ? 0
                    : list.Max(r => r.ResponseTimeMs),
                ByStatusCode = list
                    .GroupBy(r => r.StatusCode)
                    .Select(g => new StatusCodeStatDto
                    {
                        StatusCode = g.Key,
                        Count = g.Count(),
                        Percentage = total == 0 ? 0
                            : Math.Round((double)g.Count() / total * 100, 2)
                    })
                    .OrderBy(x => x.StatusCode).ToList(),
                TopEndpoints = list
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
                TopIps = list
                    .GroupBy(r => r.IpAddress)
                    .Select(g => new IpRequestStatDto
                    {
                        IpAddress = g.Key,
                        Count = g.Count(),
                        ErrorCount = g.Count(r => r.StatusCode >= 400)
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(10).ToList()
            });
        }
    }
}

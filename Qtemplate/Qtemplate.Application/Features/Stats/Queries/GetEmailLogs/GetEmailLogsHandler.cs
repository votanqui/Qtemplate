using MediatR;
using Qtemplate.Application.DTOs.Email;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qtemplate.Application.DTOs.Admin;

namespace Qtemplate.Application.Features.Stats.Queries.GetEmailLogs
{
    public class GetEmailLogsHandler
     : IRequestHandler<GetEmailLogsQuery, ApiResponse<EmailLogStatsDto>>
    {
        private readonly IStatsRepository _stats;
        public GetEmailLogsHandler(IStatsRepository stats) => _stats = stats;

        public async Task<ApiResponse<EmailLogStatsDto>> Handle(
            GetEmailLogsQuery request, CancellationToken cancellationToken)
        {
            var to = (request.To ?? DateTime.UtcNow).Date.AddDays(1).AddSeconds(-1);
            var from = (request.From ?? to.AddDays(-29)).Date;

            // Lấy theo range cho stats, all cho recent failed
            var list = await _stats.GetAllEmailLogsAsync();
            var total = list.Count;

            return ApiResponse<EmailLogStatsDto>.Ok(new EmailLogStatsDto
            {
                Total = total,
                Sent = list.Count(e => e.Status == "Sent"),
                Failed = list.Count(e => e.Status == "Failed"),
                Pending = list.Count(e => e.Status == "Pending"),
                SuccessRate = total == 0 ? 0
                    : Math.Round((double)list.Count(e => e.Status == "Sent") / total * 100, 2),
                FailureRate = total == 0 ? 0
                    : Math.Round((double)list.Count(e => e.Status == "Failed") / total * 100, 2),
                ByTemplate = list
                    .GroupBy(e => e.Template)
                    .Select(g => new EmailTemplateStatDto
                    {
                        Template = g.Key,
                        Total = g.Count(),
                        Sent = g.Count(e => e.Status == "Sent"),
                        Failed = g.Count(e => e.Status == "Failed")
                    })
                    .OrderByDescending(x => x.Total).ToList(),
                RecentFailed = list
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
            });
        }
    }
}

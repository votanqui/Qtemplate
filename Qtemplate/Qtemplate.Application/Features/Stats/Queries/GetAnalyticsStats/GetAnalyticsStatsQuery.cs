using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Stats;

namespace Qtemplate.Application.Features.Stats;

public class GetAnalyticsStatsQuery : IRequest<ApiResponse<AnalyticsStatsDto>>
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
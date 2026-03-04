using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Stats;

namespace Qtemplate.Application.Features.Stats.Queries.GetMediaStats;

public class GetMediaStatsQuery : IRequest<ApiResponse<MediaStatsDto>>
{
}
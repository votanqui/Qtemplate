using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Admin;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Stats.Queries.GetIpBlacklist;



public class GetIpBlacklistHandler
    : IRequestHandler<GetIpBlacklistQuery, ApiResponse<IpBlacklistStatsDto>>
{
    private readonly IStatsRepository _stats;
    public GetIpBlacklistHandler(IStatsRepository stats) => _stats = stats;

    public async Task<ApiResponse<IpBlacklistStatsDto>> Handle(
        GetIpBlacklistQuery request, CancellationToken cancellationToken)
    {
        var list = await _stats.GetAllIpBlacklistAsync();

        return ApiResponse<IpBlacklistStatsDto>.Ok(new IpBlacklistStatsDto
        {
            Total = list.Count,
            Active = list.Count(x => x.IsActive),
            Inactive = list.Count(x => !x.IsActive),
            Manual = list.Count(x => x.Type == "Manual"),
            Auto = list.Count(x => x.Type == "Auto"),
            Permanent = list.Count(x => x.ExpiredAt == null),
            Temporary = list.Count(x => x.ExpiredAt != null),
            RecentBlocked = list
                .OrderByDescending(x => x.BlockedAt)
                .Take(10)
                .Select(x => new RecentBlockedDto
                {
                    IpAddress = x.IpAddress,
                    Reason = x.Reason,
                    Type = x.Type,
                    BlockedAt = x.BlockedAt
                }).ToList()
        });
    }
}
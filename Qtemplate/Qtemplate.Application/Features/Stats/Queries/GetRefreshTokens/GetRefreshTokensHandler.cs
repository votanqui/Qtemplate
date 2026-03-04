using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Admin;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Stats.Queries.GetRefreshTokens;

public class GetRefreshTokensHandler
    : IRequestHandler<GetRefreshTokensQuery, ApiResponse<RefreshTokenStatsDto>>
{
    private readonly IRefreshTokenRepository _tokenRepo;
    public GetRefreshTokensHandler(IRefreshTokenRepository tokenRepo) => _tokenRepo = tokenRepo;

    public async Task<ApiResponse<RefreshTokenStatsDto>> Handle(
        GetRefreshTokensQuery request, CancellationToken cancellationToken)
    {
        // Lấy tất cả để tính stats — RefreshToken thường không quá lớn
        var (all, _) = await _tokenRepo.GetPagedAsync(null, null, 1, int.MaxValue);

        var now = DateTime.UtcNow;
        var active = all.Where(t => !t.IsRevoked && t.ExpiresAt > now).ToList();
        var revoked = all.Where(t => t.IsRevoked).ToList();
        var expired = all.Where(t => !t.IsRevoked && t.ExpiresAt <= now).ToList();

        // Detect suspicious: 1 user login từ 3+ IP khác nhau và đang active
        var suspicious = active
            .GroupBy(t => new { t.UserId, t.User?.Email })
            .Where(g => g.Select(t => t.IpAddress).Distinct().Count() >= 3)
            .Select(g => new SuspiciousTokenDto
            {
                UserId = g.Key.UserId,
                UserEmail = g.Key.Email,
                IpCount = g.Select(t => t.IpAddress).Distinct().Count(),
                IpAddresses = g.Select(t => t.IpAddress)
                               .Where(ip => ip != null)
                               .Distinct()
                               .ToList()!
            })
            .OrderByDescending(x => x.IpCount)
            .ToList();

        return ApiResponse<RefreshTokenStatsDto>.Ok(new RefreshTokenStatsDto
        {
            Total = all.Count,
            Active = active.Count,
            Revoked = revoked.Count,
            Expired = expired.Count,
            RevokedByAdmin = revoked.Count(t =>
                t.RevokedReason is "AdminLocked" or "AccountDeleted"),
            RevokedByLogout = revoked.Count(t =>
                t.RevokedReason == "Logout"),
            SuspiciousIps = suspicious
        });
    }
}
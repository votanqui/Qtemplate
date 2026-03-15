using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task AddAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
    Task RevokeAllByUserIdAsync(Guid userId, string reason);
    Task<bool> HasLoginFromIpAsync(Guid userId, string ipAddress);
    Task<(List<RefreshToken> Items, int Total)> GetPagedAsync(
    Guid? userId, bool? isActive, int page, int pageSize);
    Task RevokeAndAddAsync(RefreshToken oldToken, RefreshToken newToken);
}
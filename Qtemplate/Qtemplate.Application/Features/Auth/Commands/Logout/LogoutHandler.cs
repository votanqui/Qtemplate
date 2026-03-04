using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Auth.Commands.Logout;

public class LogoutHandler : IRequestHandler<LogoutCommand, ApiResponse<object>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepo;

    public LogoutHandler(IRefreshTokenRepository refreshTokenRepo)
    {
        _refreshTokenRepo = refreshTokenRepo;
    }

    public async Task<ApiResponse<object>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await _refreshTokenRepo.GetByTokenAsync(request.RefreshToken);
        if (token is not null)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = "Đăng xuất";
            await _refreshTokenRepo.UpdateAsync(token);
        }

        return ApiResponse<object>.Ok(null!, "Đăng xuất thành công");
    }
}
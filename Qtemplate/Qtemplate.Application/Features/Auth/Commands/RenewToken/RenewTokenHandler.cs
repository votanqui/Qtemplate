// File: Qtemplate.Application/Features/Auth/Commands/RenewToken/RenewTokenHandler.cs

using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.User;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Auth.Commands.RenewToken;

public class RefreshTokenHandler : IRequestHandler<RenewTokenCommand, ApiResponse<AuthResponseDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IJwtTokenService _jwtService;

    public RefreshTokenHandler(
        IUserRepository userRepo,
        IRefreshTokenRepository refreshTokenRepo,
        IJwtTokenService jwtService)
    {
        _userRepo = userRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _jwtService = jwtService;
    }

    public async Task<ApiResponse<AuthResponseDto>> Handle(
        RenewTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await _refreshTokenRepo.GetByTokenAsync(request.RefreshToken);

        if (token is null)
            return ApiResponse<AuthResponseDto>.Fail("Token không hợp lệ");

        if (token.IsRevoked)
            return ApiResponse<AuthResponseDto>.Fail("Token đã bị thu hồi");

        if (token.IsExpired)
            return ApiResponse<AuthResponseDto>.Fail("Token đã hết hạn, vui lòng đăng nhập lại");

        var user = await _userRepo.GetByIdAsync(token.UserId);
        if (user is null || !user.IsActive)
            return ApiResponse<AuthResponseDto>.Fail("Tài khoản không tồn tại hoặc đã bị khóa");

        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshTokenValue = _jwtService.GenerateRefreshToken();

        // ── Gộp revoke + add mới vào 1 SaveChangesAsync ──────────────────────
        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedReason = "Được thay thế bởi token mới";
        token.ReplacedByToken = newRefreshTokenValue;

        await _refreshTokenRepo.RevokeAndAddAsync(token, new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenValue,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        });

        return ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenValue,
            AccessTokenExpiry = _jwtService.GetAccessTokenExpiry(),
            User = new UserInfoDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                AvatarUrl = user.AvatarUrl
            }
        }, "Làm mới token thành công");
    }
}
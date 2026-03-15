// File: Qtemplate.Application/Features/Auth/Commands/Login/LoginHandler.cs

using MediatR;
using Microsoft.Extensions.Configuration;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.User;
using Qtemplate.Application.Services;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Domain.Messages;

namespace Qtemplate.Application.Features.Auth.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, ApiResponse<AuthResponseDto>>
{
    // Giới hạn concurrent BCrypt để tránh CPU saturation khi nhiều login đồng thời
    private static readonly SemaphoreSlim _bcryptSemaphore = new(Environment.ProcessorCount * 2);

    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IJwtTokenService _jwtService;
    private readonly IAuditLogService _auditLogService;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _config;

    public LoginHandler(
        IUserRepository userRepo,
        IRefreshTokenRepository refreshTokenRepo,
        IJwtTokenService jwtService,
        IAuditLogService auditLogService,
        IEmailSender emailSender,
        IConfiguration config)
    {
        _userRepo = userRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _jwtService = jwtService;
        _auditLogService = auditLogService;
        _emailSender = emailSender;
        _config = config;
    }

    public async Task<ApiResponse<AuthResponseDto>> Handle(
        LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email);
        if (user is null)
            return ApiResponse<AuthResponseDto>.Fail("Email hoặc mật khẩu không chính xác");

        if (!user.IsActive)
            return ApiResponse<AuthResponseDto>.Fail("Tài khoản đã bị khóa, vui lòng liên hệ hỗ trợ");

        // ── BCrypt verify — giới hạn concurrency tránh CPU saturate ──────────
        await _bcryptSemaphore.WaitAsync(cancellationToken);
        bool passwordOk;
        try
        {
            passwordOk = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        }
        finally
        {
            _bcryptSemaphore.Release();
        }

        if (!passwordOk)
        {
            // Fire-and-forget audit log — không block response
            _ = _auditLogService.LogAsync(
                userId: user.Id.ToString(),
                userEmail: user.Email,
                action: "LoginFailed",
                entityName: "User",
                entityId: user.Id.ToString(),
                ipAddress: request.IpAddress);

            return ApiResponse<AuthResponseDto>.Fail("Email hoặc mật khẩu không chính xác");
        }

        // ── Check email verify SAU BCrypt (tránh leak thông tin thứ tự) ──────
        if (!user.IsEmailVerified)
            return ApiResponse<AuthResponseDto>.Fail(
                "Email chưa được xác minh. Vui lòng kiểm tra hộp thư hoặc yêu cầu gửi lại email xác minh");

        // ── Sinh token ────────────────────────────────────────────────────────
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshTokenValue = _jwtService.GenerateRefreshToken();

        // ── Kiểm tra IP mới — fire-and-forget email cảnh báo ─────────────────
        var isNewIp = !await _refreshTokenRepo.HasLoginFromIpAsync(user.Id, request.IpAddress);
        if (isNewIp)
        {
            var supportUrl = $"{_config["App:BaseUrl"]}/support";
            _ = _emailSender.SendAsync(new SendEmailMessage
            {
                To = user.Email,
                Subject = "Cảnh báo đăng nhập từ thiết bị mới",
                Body = EmailTemplates.SuspiciousLogin(
                    user.FullName, request.IpAddress, request.UserAgent, supportUrl),
                Template = "SuspiciousLogin"
            });
        }

        // ── Gộp RefreshToken + UpdateUser vào 1 SaveChanges ──────────────────
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepo.AddRefreshTokenAndUpdateUserAsync(user, new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        });

        // Fire-and-forget audit log — không block response
        _ = _auditLogService.LogAsync(
            userId: user.Id.ToString(),
            userEmail: user.Email,
            action: "Login",
            entityName: "User",
            entityId: user.Id.ToString(),
            ipAddress: request.IpAddress);

        return ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            AccessTokenExpiry = _jwtService.GetAccessTokenExpiry(),
            User = MapToUserInfo(user)
        }, "Đăng nhập thành công");
    }

    private static UserInfoDto MapToUserInfo(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role,
        AvatarUrl = user.AvatarUrl
    };
}
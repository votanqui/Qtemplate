// Qtemplate.Application/Features/Auth/Commands/Login/LoginHandler.cs
// Bổ sung: gửi email cảnh báo đăng nhập từ địa chỉ IP/thiết bị mới
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

    public async Task<ApiResponse<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email);
        if (user is null)
            return ApiResponse<AuthResponseDto>.Fail("Email hoặc mật khẩu không chính xác");

        if (!user.IsActive)
            return ApiResponse<AuthResponseDto>.Fail("Tài khoản đã bị khóa, vui lòng liên hệ hỗ trợ");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return ApiResponse<AuthResponseDto>.Fail("Email hoặc mật khẩu không chính xác");
        if (!user.IsEmailVerified)
            return ApiResponse<AuthResponseDto>.Fail(
                "Email chưa được xác minh. Vui lòng kiểm tra hộp thư hoặc yêu cầu gửi lại email xác minh"
               );
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshTokenValue = _jwtService.GenerateRefreshToken();

        // Kiểm tra IP mới — gửi cảnh báo nếu chưa từng đăng nhập từ IP này
        var isNewIp = !await _refreshTokenRepo.HasLoginFromIpAsync(user.Id, request.IpAddress);
        if (isNewIp && user.IsEmailVerified)
        {
            var supportUrl = $"{_config["App:BaseUrl"]}/support";
            // Fire-and-forget: không block login nếu gửi mail lỗi
            _ = _emailSender.SendAsync(new SendEmailMessage
            {
                To = user.Email,
                Subject = "Cảnh báo đăng nhập từ thiết bị mới",
                Body = EmailTemplates.SuspiciousLogin(user.FullName, request.IpAddress, request.UserAgent, supportUrl),
                Template = "SuspiciousLogin"
            });
        }

        await _refreshTokenRepo.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        });

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user);

        await _auditLogService.LogAsync(
            userId: user.Id.ToString(),
            userEmail: user.Email,
            action: "Login",
            entityName: "User",
            entityId: user.Id.ToString(),
            ipAddress: request.IpAddress
        );

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
// Qtemplate.Application/Features/Auth/Commands/Register/RegisterHandler.cs
using MediatR;
using Microsoft.Extensions.Configuration;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.User;
using Qtemplate.Application.Services;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Domain.Messages;

namespace Qtemplate.Application.Features.Auth.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, ApiResponse<AuthResponseDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IJwtTokenService _jwtService;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _config;

    public RegisterHandler(
        IUserRepository userRepo,
        IRefreshTokenRepository refreshTokenRepo,
        IJwtTokenService jwtService,
        IEmailSender emailSender,
        IConfiguration config)
    {
        _userRepo = userRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _jwtService = jwtService;
        _emailSender = emailSender;
        _config = config;
    }

    public async Task<ApiResponse<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (request.Password != request.ConfirmPassword)
            return ApiResponse<AuthResponseDto>.Fail("Mật khẩu xác nhận không khớp");

        // Sanitize email ngay từ đầu
        var email = request.Email.ToLower().Trim();

        if (await _userRepo.EmailExistsAsync(email))
            return ApiResponse<AuthResponseDto>.Fail("Email này đã được sử dụng");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "Customer",
            IsActive = true,
            IsEmailVerified = false,
            EmailVerifyToken = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user);

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshTokenValue = _jwtService.GenerateRefreshToken();

        await _refreshTokenRepo.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        });

        // Gửi email sau khi đã lưu user và token — fire-and-forget để không block response
        var verifyUrl = $"{_config["App:BaseUrl"]}/api/auth/verifyemail?token={user.EmailVerifyToken}";
        _ = _emailSender.SendAsync(new SendEmailMessage
        {
            To = user.Email,
            Subject = "Xác minh tài khoản của bạn",
            Body = EmailTemplates.VerifyEmail(user.FullName, verifyUrl),
            Template = "VerifyEmail"
        });

        return ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            AccessTokenExpiry = _jwtService.GetAccessTokenExpiry(),
            User = new UserInfoDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                AvatarUrl = user.AvatarUrl
            }
        }, "Đăng ký thành công, vui lòng kiểm tra email để xác thực tài khoản");
    }
}
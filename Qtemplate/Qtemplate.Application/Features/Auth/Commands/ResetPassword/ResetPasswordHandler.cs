using MediatR;
using Microsoft.Extensions.Configuration;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Domain.Messages;

namespace Qtemplate.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, ApiResponse<object>>
{
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _config;

    public ResetPasswordHandler(
        IUserRepository userRepo,
        IRefreshTokenRepository refreshTokenRepo,
        IEmailSender emailSender,
        IConfiguration config)
    {
        _userRepo = userRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _emailSender = emailSender;
        _config = config;
    }

    public async Task<ApiResponse<object>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return ApiResponse<object>.Fail("Mật khẩu xác nhận không khớp");

        var user = await _userRepo.GetByResetTokenAsync(request.Token);
        if (user is null)
            return ApiResponse<object>.Fail("Token không hợp lệ");

        if (user.ResetPasswordExpiry < DateTime.UtcNow)
            return ApiResponse<object>.Fail("Token đã hết hạn, vui lòng yêu cầu lại");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.ResetPasswordToken = null;
        user.ResetPasswordExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user);

        await _refreshTokenRepo.RevokeAllByUserIdAsync(user.Id, "Đặt lại mật khẩu");

        // Gửi email thông báo đặt lại mật khẩu thành công
        var supportUrl = $"{_config["App:BaseUrl"]}/support";
        _ = _emailSender.SendAsync(new SendEmailMessage
        {
            To = user.Email,
            Subject = "Mật khẩu của bạn đã được thay đổi",
            Body = EmailTemplates.PasswordChanged(user.FullName, "Không xác định", supportUrl),
            Template = "PasswordChanged"
        });

        return ApiResponse<object>.Ok(null!, "Đặt lại mật khẩu thành công, vui lòng đăng nhập lại");
    }
}
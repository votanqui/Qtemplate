// Qtemplate.Application/Features/Auth/Commands/ChangePassword/ChangePasswordHandler.cs
// Bổ sung: gửi email thông báo sau khi đổi mật khẩu thành công
using MediatR;
using Microsoft.Extensions.Configuration;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Domain.Messages;

namespace Qtemplate.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, ApiResponse<object>>
{
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _config;

    public ChangePasswordHandler(
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

    public async Task<ApiResponse<object>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId);
        if (user is null)
            return ApiResponse<object>.Fail("Không tìm thấy người dùng");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return ApiResponse<object>.Fail("Mật khẩu hiện tại không chính xác");

        if (request.NewPassword != request.ConfirmPassword)
            return ApiResponse<object>.Fail("Mật khẩu mới xác nhận không khớp");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user);

        // Thu hồi toàn bộ refresh token (bắt đăng nhập lại trên tất cả thiết bị)
        await _refreshTokenRepo.RevokeAllByUserIdAsync(user.Id, "PasswordChanged");

        // Gửi email thông báo
        var supportUrl = $"{_config["App:BaseUrl"]}/support";
        _ = _emailSender.SendAsync(new SendEmailMessage
        {
            To = user.Email,
            Subject = "Mật khẩu của bạn đã được thay đổi",
            Body = EmailTemplates.PasswordChanged(user.FullName, request.IpAddress ?? "Không xác định", supportUrl),
            Template = "PasswordChanged"
        });
        return ApiResponse<object>.Ok(null!, "Thay đổi mật khẩu thành công");
    }
}
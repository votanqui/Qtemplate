using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Domain.Messages;
using Microsoft.Extensions.Configuration;

namespace Qtemplate.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, ApiResponse<object>>
{
    private readonly IUserRepository _userRepo;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _config;

    public ForgotPasswordHandler(
        IUserRepository userRepo,
        IEmailSender emailSender,   // ← đúng type
        IConfiguration config)
    {
        _userRepo = userRepo;
        _emailSender = emailSender;
        _config = config;
    }

    public async Task<ApiResponse<object>> Handle(
        ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email);

        if (user is null || !user.IsActive)
            return ApiResponse<object>.Ok(null!,
                "Nếu email tồn tại, chúng tôi đã gửi hướng dẫn đặt lại mật khẩu");
        if (!user.IsEmailVerified)
            return ApiResponse<object>.Fail(
                "Email chưa được xác minh. Vui lòng xác minh email trước khi đặt lại mật khẩu");
        user.ResetPasswordToken = Guid.NewGuid().ToString("N");
        user.ResetPasswordExpiry = DateTime.UtcNow.AddHours(1);
        await _userRepo.UpdateAsync(user);

        var resetUrl = $"{_config["App:BaseUrl"]}/reset-password?token={user.ResetPasswordToken}";

        _ = _emailSender.SendAsync(new SendEmailMessage
        {
            To = user.Email,
            Subject = "Đặt lại mật khẩu",
            Body = EmailTemplates.ForgotPassword(user.FullName, resetUrl),
            Template = "ForgotPassword"
        });

        return ApiResponse<object>.Ok(null!,
            "Nếu email tồn tại, chúng tôi đã gửi hướng dẫn đặt lại mật khẩu");
    }
}
// Qtemplate.Application/Features/Auth/Commands/VerifyEmail/VerifyEmailHandler.cs
using MediatR;
using Microsoft.Extensions.Configuration;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Domain.Messages;

namespace Qtemplate.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailHandler : IRequestHandler<VerifyEmailCommand, ApiResponse<object>>
{
    private readonly IUserRepository _userRepo;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _config;

    public VerifyEmailHandler(
        IUserRepository userRepo,
        IEmailSender emailSender,
        IConfiguration config)
    {
        _userRepo = userRepo;
        _emailSender = emailSender;
        _config = config;
    }

    public async Task<ApiResponse<object>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByEmailVerifyTokenAsync(request.Token);
        if (user is null)
            return ApiResponse<object>.Fail("Token xác thực không hợp lệ hoặc đã hết hạn");

        if (user.IsEmailVerified)
            return ApiResponse<object>.Ok(null!, "Email đã được xác thực trước đó");

        user.IsEmailVerified = true;
        user.EmailVerifyToken = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user);

        // Gửi email chào mừng sau khi xác thực thành công
        var loginUrl = $"{_config["App:BaseUrl"]}/login";
        _ = _emailSender.SendAsync(new SendEmailMessage
        {
            To = user.Email,
            Subject = "Chào mừng bạn đến với Qtemplate! 🎉",
            Body = EmailTemplates.WelcomeAfterVerify(user.FullName, loginUrl),
            Template = "WelcomeAfterVerify"
        });

        return ApiResponse<object>.Ok(null!, "Xác thực email thành công, chào mừng bạn đến với Qtemplate!");
    }
}
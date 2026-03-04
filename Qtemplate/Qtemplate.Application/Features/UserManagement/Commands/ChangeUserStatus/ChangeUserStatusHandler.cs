using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Domain.Messages;

namespace Qtemplate.Application.Features.Admin.Users.Commands.ChangeUserStatus;

public class ChangeUserStatusHandler : IRequestHandler<ChangeUserStatusCommand, ApiResponse<object>>
{
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _tokenRepo;
    private readonly IAuditLogService _auditLogService;
    private readonly IEmailSender _emailSender;

    public ChangeUserStatusHandler(
        IUserRepository userRepo,
        IRefreshTokenRepository tokenRepo,
        IAuditLogService auditLogService,
        IEmailSender emailSender)
    {
        _userRepo = userRepo;
        _tokenRepo = tokenRepo;
        _auditLogService = auditLogService;
        _emailSender = emailSender;
    }

    public async Task<ApiResponse<object>> Handle(
        ChangeUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(request.TargetUserId);
        if (user is null)
            return ApiResponse<object>.Fail("Không tìm thấy người dùng");

        if (user.Role == "Admin")
            return ApiResponse<object>.Fail("Không thể thay đổi trạng thái tài khoản Admin");

        var oldStatus = user.IsActive;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user);

        if (!request.IsActive)
            await _tokenRepo.RevokeAllByUserIdAsync(user.Id, request.Reason ?? "AdminLocked");

        await _auditLogService.LogAsync(
            userId: request.AdminId,
            userEmail: request.AdminEmail,
            action: request.IsActive ? "UnlockUser" : "LockUser",
            entityName: "User",
            entityId: user.Id.ToString(),
            oldValues: new { IsActive = oldStatus },
            newValues: new { IsActive = request.IsActive, request.Reason },
            ipAddress: request.IpAddress);

        // Gửi email thông báo khoá / mở khoá tài khoản
        _ = _emailSender.SendAsync(new SendEmailMessage
        {
            To = user.Email,
            Subject = request.IsActive
                ? "Tài khoản của bạn đã được mở khoá"
                : "Tài khoản của bạn đã bị khoá",
            Body = request.IsActive
                ? EmailTemplates.AccountUnlocked(user.FullName)
                : EmailTemplates.AccountLocked(user.FullName, request.Reason),
            Template = request.IsActive ? "AccountUnlocked" : "AccountLocked"
        });

        return ApiResponse<object>.Ok(null!,
            request.IsActive ? "Đã mở khoá tài khoản" : "Đã khoá tài khoản");
    }
}
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
    private readonly INotificationService _notifService;
    private readonly ISecurityScanLogRepository _scanLogRepo;

    public ChangeUserStatusHandler(
        IUserRepository userRepo,
        IRefreshTokenRepository tokenRepo,
        IAuditLogService auditLogService,
        IEmailSender emailSender,
        INotificationService notifService,
        ISecurityScanLogRepository scanLogRepo)
    {
        _userRepo = userRepo;
        _tokenRepo = tokenRepo;
        _auditLogService = auditLogService;
        _emailSender = emailSender;
        _notifService = notifService;
        _scanLogRepo = scanLogRepo;
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

        // Khi admin chủ động MỞ KHOÁ → đánh dấu ScanLog của user này
        // là IsAdminOverride = true để scanner không khoá lại trong cửa sổ hiện tại
        if (request.IsActive)
            await MarkScanLogsOverriddenAsync(request.TargetUserId, request.AdminEmail, request.Reason);

        await _auditLogService.LogAsync(
            userId: request.AdminId,
            userEmail: request.AdminEmail,
            action: request.IsActive ? "UnlockUser" : "LockUser",
            entityName: "User",
            entityId: user.Id.ToString(),
            oldValues: new { IsActive = oldStatus },
            newValues: new { IsActive = request.IsActive, request.Reason },
            ipAddress: request.IpAddress);

        await _notifService.SendToUserAsync(
            user.Id,
            title: request.IsActive ? "Tài khoản đã được mở khoá" : "Tài khoản đã bị khoá",
            message: request.IsActive
                ? "Tài khoản của bạn đã được khôi phục, bạn có thể đăng nhập bình thường."
                : $"Tài khoản của bạn đã bị khoá. Lý do: {request.Reason}",
            type: request.IsActive ? "Success" : "Warning");

        _ = _emailSender.SendAsync(new SendEmailMessage
        {
            To = user.Email,
            Subject = request.IsActive ? "Tài khoản của bạn đã được mở khoá" : "Tài khoản của bạn đã bị khoá",
            Body = request.IsActive
                ? EmailTemplates.AccountUnlocked(user.FullName)
                : EmailTemplates.AccountLocked(user.FullName, request.Reason),
            Template = request.IsActive ? "AccountUnlocked" : "AccountLocked"
        });

        return ApiResponse<object>.Ok(null!,
            request.IsActive ? "Đã mở khoá tài khoản" : "Đã khoá tài khoản");
    }

    private async Task MarkScanLogsOverriddenAsync(Guid userId, string? adminEmail, string? note)
    {
        var windowFrom = DateTime.UtcNow.AddHours(-24); // bao phủ cửa sổ tối đa
        var (logs, _) = await _scanLogRepo.GetPagedAsync(
            violation: null, userId: userId, ipAddress: null,
            isOverride: false, page: 1, pageSize: 100);

        foreach (var log in logs.Where(l => l.ScannedAt >= windowFrom))
        {
            log.IsAdminOverride = true;
            log.OverrideByEmail = adminEmail;
            log.OverrideNote = note ?? "Admin mở khoá tài khoản thủ công";
            log.OverrideAt = DateTime.UtcNow;
            await _scanLogRepo.UpdateAsync(log);
        }
    }
}
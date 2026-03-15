using Microsoft.Extensions.Logging;
using Qtemplate.Application.Constants;
using Qtemplate.Application.Services;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Infrastructure.Services.Security;

public class SuspiciousBehaviorScanner
{
    private readonly ISettingRepository _settingRepo;
    private readonly IRequestLogRepository _requestLogRepo;
    private readonly IReviewRepository _reviewRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly IIpBlacklistRepository _ipBlacklistRepo;
    private readonly IUserRepository _userRepo;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly IAuditLogService _auditLogService;
    private readonly ISecurityScanLogRepository _scanLogRepo;
    private readonly ILogger<SuspiciousBehaviorScanner> _logger;

    public SuspiciousBehaviorScanner(
        ISettingRepository settingRepo,
        IRequestLogRepository requestLogRepo,
        IReviewRepository reviewRepo,
        IOrderRepository orderRepo,
        IIpBlacklistRepository ipBlacklistRepo,
        IUserRepository userRepo,
        INotificationService notificationService,
        IEmailService emailService,
        IAuditLogService auditLogService,
        ISecurityScanLogRepository scanLogRepo,
        ILogger<SuspiciousBehaviorScanner> logger)
    {
        _settingRepo = settingRepo;
        _requestLogRepo = requestLogRepo;
        _reviewRepo = reviewRepo;
        _orderRepo = orderRepo;
        _ipBlacklistRepo = ipBlacklistRepo;
        _userRepo = userRepo;
        _notificationService = notificationService;
        _emailService = emailService;
        _auditLogService = auditLogService;
        _scanLogRepo = scanLogRepo;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        int windowMin = await _settingRepo.GetIntAsync(SettingKeys.SecurityTimeWindowMinutes, 60);
        int maxRequests = await _settingRepo.GetIntAsync(SettingKeys.SecurityMaxRequestsPerWindow, 500);
        int maxFailedLogins = await _settingRepo.GetIntAsync(SettingKeys.SecurityMaxFailedLogins, 10);
        int maxErrorPct = await _settingRepo.GetIntAsync(SettingKeys.SecurityMaxErrorRatePercent, 40);
        int maxScanReqs = await _settingRepo.GetIntAsync(SettingKeys.SecurityMaxScanRequests, 30);
        int maxReviewSpam = await _settingRepo.GetIntAsync(SettingKeys.SecurityMaxReviewSpam, 5);
        int maxOrderCancel = await _settingRepo.GetIntAsync(SettingKeys.SecurityMaxOrderCancels, 5);
        int blockHours = await _settingRepo.GetIntAsync(SettingKeys.SecurityBlockDurationHours, 24);

        var windowFrom = DateTime.UtcNow.AddMinutes(-windowMin);
        DateTime? expiredAt = blockHours > 0 ? DateTime.UtcNow.AddHours(blockHours) : null;

        // ── 1. IP flood ───────────────────────────────────────────────────────
        foreach (var (ip, userId, count) in await _requestLogRepo.GetHighVolumeAsync(windowFrom, maxRequests))
        {
            ct.ThrowIfCancellationRequested();
            await HandleIpAsync(ip, userId, windowFrom, expiredAt,
                violation: "IpFlood",
                reason: $"Gửi {count} request trong {windowMin} phút (ngưỡng: {maxRequests}).",
                notifTitle: "Hoạt động bất thường: lưu lượng truy cập cao",
                notifMessage: $"Phát hiện {count} request liên tiếp từ tài khoản của bạn trong thời gian ngắn.");
        }

        // ── 2. IP tỷ lệ lỗi cao ───────────────────────────────────────────────
        foreach (var (ip, userId, errorPct) in await _requestLogRepo.GetHighErrorRateAsync(windowFrom, 10, maxErrorPct))
        {
            ct.ThrowIfCancellationRequested();
            await HandleIpAsync(ip, userId, windowFrom, expiredAt,
                violation: "IpHighErrorRate",
                reason: $"Tỷ lệ lỗi HTTP {errorPct}% trong {windowMin} phút (ngưỡng: {maxErrorPct}%).",
                notifTitle: "Hoạt động bất thường: tỷ lệ lỗi cao",
                notifMessage: $"Phát hiện tỷ lệ lỗi {errorPct}% từ tài khoản của bạn.");
        }

        // ── 3. IP quét endpoint ────────────────────────────────────────────────
        foreach (var (ip, userId, count) in await _requestLogRepo.GetEndpointScanAsync(windowFrom, maxScanReqs))
        {
            ct.ThrowIfCancellationRequested();
            await HandleIpAsync(ip, userId, windowFrom, expiredAt,
                violation: "IpEndpointScan",
                reason: $"Quét {count} endpoint không tồn tại trong {windowMin} phút (ngưỡng: {maxScanReqs}).",
                notifTitle: "Hoạt động bất thường: quét hệ thống",
                notifMessage: $"Phát hiện {count} yêu cầu tới đường dẫn không tồn tại từ tài khoản của bạn.");
        }

        // ── 4. Brute-force login ──────────────────────────────────────────────
        foreach (var (userId, count) in await GetFailedLoginUsersAsync(windowFrom, maxFailedLogins))
        {
            ct.ThrowIfCancellationRequested();
            await HandleUserAsync(userId, windowFrom, expiredAt,
                violation: "BruteForceLogin",
                reason: $"Đăng nhập thất bại {count} lần trong {windowMin} phút (ngưỡng: {maxFailedLogins}).",
                notifTitle: "Tài khoản bị khoá: đăng nhập thất bại quá nhiều lần",
                notifMessage: $"Ghi nhận {count} lần đăng nhập thất bại liên tiếp. Tài khoản đã bị tạm khoá.");
        }

        // ── 5. Review spam ────────────────────────────────────────────────────
        foreach (var (userId, count) in await _reviewRepo.GetSpamUsersAsync(windowFrom, maxReviewSpam))
        {
            ct.ThrowIfCancellationRequested();
            await HandleUserAsync(userId, windowFrom, expiredAt,
                violation: "ReviewSpam",
                reason: $"Gửi {count} đánh giá trong {windowMin} phút (ngưỡng: {maxReviewSpam}).",
                notifTitle: "Tài khoản bị khoá: spam đánh giá",
                notifMessage: $"Phát hiện {count} đánh giá gửi liên tiếp trong thời gian ngắn.");
        }

        // ── 6. Order cancel spam ──────────────────────────────────────────────
        foreach (var (userId, count) in await _orderRepo.GetCancelSpamUsersAsync(windowFrom, maxOrderCancel))
        {
            ct.ThrowIfCancellationRequested();
            await HandleUserAsync(userId, windowFrom, expiredAt,
                violation: "OrderCancelSpam",
                reason: $"Huỷ {count} đơn hàng trong {windowMin} phút (ngưỡng: {maxOrderCancel}).",
                notifTitle: "Tài khoản bị khoá: huỷ đơn hàng bất thường",
                notifMessage: $"Ghi nhận {count} lần huỷ đơn hàng liên tiếp từ tài khoản của bạn.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────

    private async Task HandleIpAsync(
        string ip, string? userIdStr, DateTime windowFrom, DateTime? expiredAt,
        string violation, string reason, string notifTitle, string notifMessage)
    {
        Guid? userId = Guid.TryParse(userIdStr, out var uid) ? uid : null;

        // Đã xử lý trong cửa sổ này rồi (dù admin override hay chưa) → bỏ qua
        if (await _scanLogRepo.IsAlreadyHandledAsync(violation, ip, userId, windowFrom))
        {
            _logger.LogDebug("[Scanner] Skip {V} IP={IP} — already in ScanLog.", violation, ip);
            return;
        }

        await BlockIpAsync(ip, reason, expiredAt);

        var action = "BlockIp";
        if (userId.HasValue)
        {
            await BlockUserAsync(userId.Value, reason, expiredAt, notifTitle, notifMessage);
            action = "BlockIpAndUser";
        }

        await _scanLogRepo.AddAsync(new SecurityScanLog
        {
            Violation = violation,
            IpAddress = ip,
            UserId = userId,
            Reason = reason,
            Action = action,
            ScannedAt = DateTime.UtcNow
        });
    }

    private async Task HandleUserAsync(
        Guid userId, DateTime windowFrom, DateTime? expiredAt,
        string violation, string reason, string notifTitle, string notifMessage)
    {
        // Đã xử lý trong cửa sổ này rồi (dù admin override hay chưa) → bỏ qua
        if (await _scanLogRepo.IsAlreadyHandledAsync(violation, ipAddress: null, userId, windowFrom))
        {
            _logger.LogDebug("[Scanner] Skip {V} User={U} — already in ScanLog.", violation, userId);
            return;
        }

        await BlockUserAsync(userId, reason, expiredAt, notifTitle, notifMessage);

        await _scanLogRepo.AddAsync(new SecurityScanLog
        {
            Violation = violation,
            IpAddress = null,
            UserId = userId,
            Reason = reason,
            Action = "BlockUser",
            ScannedAt = DateTime.UtcNow
        });
    }

    private async Task BlockUserAsync(
        Guid userId, string reason, DateTime? expiredAt,
        string notifTitle, string notifMessage)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user is null) return;

        if (user.IsActive)
        {
            user.IsActive = false;
            user.BlockedUntil = expiredAt;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user);

            await _auditLogService.LogAsync(
                userId: "SYSTEM",
                userEmail: "security-scanner@system",
                action: "AutoBlockUser",
                entityName: "User",
                entityId: userId.ToString(),
                newValues: new { Reason = reason, BlockedAt = DateTime.UtcNow, ExpiredAt = expiredAt });

            _logger.LogWarning("[Scanner] BLOCKED User {Id} ({Email}) — {Reason}",
                userId, user.Email, reason);
        }

        await _notificationService.SendToUserAsync(userId,
            title: notifTitle,
            message: notifMessage,
            type: "Warning",
            redirectUrl: "/account/security");

        string blockNote = expiredAt.HasValue
            ? $"Tài khoản sẽ tự mở khoá vào <strong>{expiredAt.Value:dd/MM/yyyy HH:mm} UTC</strong>."
            : "Tài khoản bị khoá cho đến khi bạn liên hệ hỗ trợ.";

        await _emailService.SendAsync(
            toEmail: user.Email,
            subject: $"[Qtemplate] {notifTitle}",
            htmlBody: EmailTemplates.AccountSuspended(user.FullName, reason, blockNote),
            template: "AccountSuspended");
    }

    private async Task BlockIpAsync(string ip, string reason, DateTime? expiredAt)
    {
        var existing = await _ipBlacklistRepo.GetByIpAsync(ip);
        if (existing is { IsActive: true }) return;

        if (existing is not null)
        {
            existing.IsActive = true;
            existing.Reason = reason;
            existing.BlockedAt = DateTime.UtcNow;
            existing.ExpiredAt = expiredAt;
            existing.Type = "Auto";
            await _ipBlacklistRepo.UpdateAsync(existing);
        }
        else
        {
            await _ipBlacklistRepo.AddAsync(new IpBlacklist
            {
                IpAddress = ip,
                Reason = reason,
                Type = "Auto",
                IsActive = true,
                ExpiredAt = expiredAt,
                BlockedAt = DateTime.UtcNow
            });
        }

        await _auditLogService.LogAsync(
            userId: "SYSTEM",
            userEmail: "security-scanner@system",
            action: "AutoBlockIp",
            entityName: "IpBlacklist",
            entityId: ip,
            newValues: new { Reason = reason, BlockedAt = DateTime.UtcNow, ExpiredAt = expiredAt });

        _logger.LogWarning("[Scanner] BLOCKED IP {IP} — {Reason}", ip, reason);
    }

    private async Task<List<(Guid UserId, int Count)>> GetFailedLoginUsersAsync(
        DateTime from, int threshold)
    {
        // AuditLog.Action = "LoginFailed" được ghi bởi LoginHandler
        // Cần method query trên AppDbContext trực tiếp vì IAuditLogService chỉ có LogAsync
        // → thêm vào IStatsRepository hoặc tạo IAuditLogRepository nếu cần
        // Hiện tại để trống — bổ sung khi có IAuditLogRepository
        return new List<(Guid, int)>();
    }
}
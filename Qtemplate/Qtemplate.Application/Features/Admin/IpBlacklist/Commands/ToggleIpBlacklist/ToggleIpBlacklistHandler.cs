using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Admin.IpBlacklist.Commands.ToggleIpBlacklist;

public class ToggleIpBlacklistHandler : IRequestHandler<ToggleIpBlacklistCommand, ApiResponse<object>>
{
    private readonly IIpBlacklistRepository _repo;
    private readonly ISecurityScanLogRepository _scanLogRepo;

    public ToggleIpBlacklistHandler(
        IIpBlacklistRepository repo,
        ISecurityScanLogRepository scanLogRepo)
    {
        _repo = repo;
        _scanLogRepo = scanLogRepo;
    }

    public async Task<ApiResponse<object>> Handle(
        ToggleIpBlacklistCommand request, CancellationToken cancellationToken)
    {
        var entry = await _repo.GetByIdAsync(request.Id);
        if (entry is null)
            return ApiResponse<object>.Fail("Không tìm thấy IP");

        entry.IsActive = !entry.IsActive;
        await _repo.UpdateAsync(entry);
        if (!entry.IsActive)
            await MarkScanLogsOverriddenAsync(entry.IpAddress, request.AdminEmail, request.Note);

        return ApiResponse<object>.Ok(null!,
            entry.IsActive ? "Đã kích hoạt lại block" : "Đã tắt block IP");
    }

    private async Task MarkScanLogsOverriddenAsync(string ip, string? adminEmail, string? note)
    {
        var windowFrom = DateTime.UtcNow.AddHours(-24); // bao phủ cửa sổ tối đa
        var (logs, _) = await _scanLogRepo.GetPagedAsync(
            violation: null, userId: null, ipAddress: ip,
            isOverride: false, page: 1, pageSize: 100);

        foreach (var log in logs.Where(l => l.ScannedAt >= windowFrom))
        {
            log.IsAdminOverride = true;
            log.OverrideByEmail = adminEmail;
            log.OverrideNote = note ?? "Admin mở khoá IP thủ công";
            log.OverrideAt = DateTime.UtcNow;
            await _scanLogRepo.UpdateAsync(log);
        }
    }
}
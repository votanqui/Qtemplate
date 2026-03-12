using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface IRequestLogRepository
{
    Task AddAsync(RequestLog log);
    Task<(List<RequestLog> Items, int Total)> GetPagedAsync(
        string? ip, string? userId, string? endpoint,
        int? statusCode, int page, int pageSize);

    // ── Security scanner ─────────────────────────────────────────────────────
    /// <summary>IP + số request vượt ngưỡng trong cửa sổ thời gian.</summary>
    Task<List<(string IpAddress, string? UserId, int Count)>> GetHighVolumeAsync(
        DateTime from, int threshold);

    /// <summary>IP có % lỗi (status >= 400) vượt ngưỡng. minTotal tránh tính IP ít request.</summary>
    Task<List<(string IpAddress, string? UserId, int ErrorPercent)>> GetHighErrorRateAsync(
        DateTime from, int minTotal, int errorPctThreshold);

    /// <summary>IP nhận 404 nhiều lần liên tiếp (dò endpoint).</summary>
    Task<List<(string IpAddress, string? UserId, int Count)>> GetEndpointScanAsync(
        DateTime from, int threshold);
}
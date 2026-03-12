using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface ISecurityScanLogRepository
{
    Task AddAsync(SecurityScanLog log);
    Task UpdateAsync(SecurityScanLog log);
    Task<SecurityScanLog?> GetByIdAsync(long id);
    Task<bool> IsAlreadyHandledAsync(
        string violation, string? ipAddress, Guid? userId, DateTime windowFrom);

    Task<(List<SecurityScanLog> Items, int Total)> GetPagedAsync(
        string? violation, Guid? userId, string? ipAddress,
        bool? isOverride, int page, int pageSize);
}
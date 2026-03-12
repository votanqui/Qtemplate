using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface IAuditLogRepository
{
    Task<(List<AuditLog> Items, int Total)> GetPagedAsync(
        string? userEmail,
        string? action,
        string? entityName,
        string? entityId,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize);
}
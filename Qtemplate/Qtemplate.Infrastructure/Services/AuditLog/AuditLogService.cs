using System.Text.Json;
using Qtemplate.Application.Services.Interfaces;
using AuditLogEntity = Qtemplate.Domain.Entities.AuditLog;

namespace Qtemplate.Infrastructure.Services.AuditLog;

public class AuditLogService : IAuditLogService
{
    private readonly AuditLogQueue _queue;

    public AuditLogService(AuditLogQueue queue) => _queue = queue;

    public Task LogAsync(
        string? userId,
        string? userEmail,
        string action,
        string entityName,
        string? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        string? ipAddress = null)
    {
        _queue.Enqueue(new AuditLogEntity
        {
            UserId = userId,
            UserEmail = userEmail,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        });

        return Task.CompletedTask;
    }
}
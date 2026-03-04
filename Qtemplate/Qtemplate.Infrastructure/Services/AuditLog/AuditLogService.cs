using System.Text.Json;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Infrastructure.Data;
using AuditLogEntity = Qtemplate.Domain.Entities.AuditLog;

namespace Qtemplate.Infrastructure.Services.AuditLog;

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _context;

    public AuditLogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(
        string? userId,
        string? userEmail,
        string action,
        string entityName,
        string? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        string? ipAddress = null)
    {
        var log = new AuditLogEntity
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
        };

        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}
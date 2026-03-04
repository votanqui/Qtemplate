namespace Qtemplate.Application.Services.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(
        string? userId,
        string? userEmail,
        string action,
        string entityName,
        string? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        string? ipAddress = null);
}
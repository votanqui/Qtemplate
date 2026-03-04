using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface IRequestLogRepository
{
    Task AddAsync(RequestLog log);
    Task<(List<RequestLog> Items, int Total)> GetPagedAsync(
        string? ip, string? userId, string? endpoint,
        int? statusCode, int page, int pageSize);
}
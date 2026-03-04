using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface IEmailLogRepository
{
    Task AddAsync(EmailLog log);
    Task UpdateAsync(EmailLog log);
    Task<EmailLog?> GetByIdAsync(long id);
    Task<(List<EmailLog> Items, int Total)> GetPagedAsync(
        string? status, string? template, int page, int pageSize);
    Task<List<EmailLog>> GetPendingAsync(int limit = 10);
}
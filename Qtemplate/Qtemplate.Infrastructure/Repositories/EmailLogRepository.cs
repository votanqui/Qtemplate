using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class EmailLogRepository : IEmailLogRepository
{
    private readonly AppDbContext _db;

    public EmailLogRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(EmailLog log)
    {
        await _db.EmailLogs.AddAsync(log);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(EmailLog log)
    {
        _db.EmailLogs.Update(log);
        await _db.SaveChangesAsync();
    }

    public async Task<EmailLog?> GetByIdAsync(long id) =>
        await _db.EmailLogs.FindAsync(id);

    public async Task<(List<EmailLog> Items, int Total)> GetPagedAsync(
        string? status, string? template, int page, int pageSize)
    {
        var query = _db.EmailLogs.AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(e => e.Status == status);

        if (!string.IsNullOrEmpty(template))
            query = query.Where(e => e.Template == template);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<List<EmailLog>> GetPendingAsync(int limit = 10) =>
        await _db.EmailLogs
            .Where(e => e.Status == "Pending" && e.RetryCount < 3)
            .OrderBy(e => e.CreatedAt)
            .Take(limit)
            .ToListAsync();
}
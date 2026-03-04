using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly AppDbContext _db;
    public AnalyticsRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Analytics analytics)
    {
        await _db.Analytics.AddAsync(analytics);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateTimeOnPageAsync(string sessionId, string pageUrl, int seconds)
    {
        var record = await _db.Analytics
            .Where(a => a.SessionId == sessionId && a.PageUrl == pageUrl)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync();

        if (record is not null)
        {
            record.TimeOnPage = seconds;
            await _db.SaveChangesAsync();
        }
    }
    public async Task<List<Analytics>> GetByDateRangeAsync(DateTime from, DateTime to) =>
    await _db.Analytics
        .Where(a => a.CreatedAt >= from && a.CreatedAt <= to)
        .ToListAsync();
}
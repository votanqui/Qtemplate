using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _db;
    public TicketRepository(AppDbContext db) => _db = db;

    public async Task<SupportTicket?> GetByIdAsync(int id) =>
        await _db.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Template)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<SupportTicket?> GetByIdWithRepliesAsync(int id) =>
        await _db.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Template)
            .Include(t => t.Replies)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<(List<SupportTicket> Items, int Total)> GetByUserIdAsync(
        Guid userId, int page, int pageSize)
    {
        var query = _db.SupportTickets
            .Include(t => t.Template)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(List<SupportTicket> Items, int Total)> GetAdminListAsync(
        string? status, string? priority, int page, int pageSize)
    {
        var query = _db.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Template)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(t => t.Status == status);

        if (!string.IsNullOrEmpty(priority))
            query = query.Where(t => t.Priority == priority);

        // Sắp xếp: Urgent → High → Normal → Low, mới nhất trước
        query = query
            .OrderBy(t => t.Status == "Closed" ? 1 : 0)
            .ThenBy(t => t.Priority == "Urgent" ? 0
                       : t.Priority == "High" ? 1
                       : t.Priority == "Normal" ? 2 : 3)
            .ThenByDescending(t => t.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task AddAsync(SupportTicket ticket)
    {
        await _db.SupportTickets.AddAsync(ticket);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(SupportTicket ticket)
    {
        _db.SupportTickets.Update(ticket);
        await _db.SaveChangesAsync();
    }

    public async Task AddReplyAsync(TicketReply reply)
    {
        await _db.TicketReplies.AddAsync(reply);
        await _db.SaveChangesAsync();
    }

    public async Task<string> GenerateTicketCodeAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var prefix = $"TK-{today}-";
        var todayCount = await _db.SupportTickets
            .CountAsync(t => t.TicketCode.StartsWith(prefix));
        return $"{prefix}{(todayCount + 1):D3}";
    }
    public async Task<List<SupportTicket>> GetPendingAiAsync(int limit = 10) =>
        await _db.SupportTickets
            .Where(t => !t.AiProcessed)
            .OrderBy(t => t.CreatedAt)
            .Take(limit)
            .ToListAsync();
}
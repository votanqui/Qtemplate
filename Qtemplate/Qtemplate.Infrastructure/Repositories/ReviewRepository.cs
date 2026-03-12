using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _db;
    public ReviewRepository(AppDbContext db) => _db = db;

    public async Task<Review?> GetByIdAsync(int id) =>
        await _db.Reviews
            .Include(r => r.User)
            .Include(r => r.Template)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<(List<Review> Items, int Total)> GetByTemplateSlugAsync(
        string slug, int page, int pageSize)
    {
        var query = _db.Reviews
            .Include(r => r.User)
            .Where(r => r.Template.Slug == slug && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(List<Review> Items, int Total)> GetAdminListAsync(
        string? status, int page, int pageSize)
    {
        var query = _db.Reviews
            .Include(r => r.User)
            .Include(r => r.Template)
            .AsQueryable();

        query = status switch
        {
            "pending" => query.Where(r => !r.IsApproved && r.AiStatus != "Rejected"),
            "approved" => query.Where(r => r.IsApproved),
            "rejected" => query.Where(r => r.AiStatus == "Rejected"),
            _ => query
        };

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Review?> GetByUserAndTemplateAsync(Guid userId, Guid templateId) =>
        await _db.Reviews.FirstOrDefaultAsync(
            r => r.UserId == userId && r.TemplateId == templateId);

    public async Task<List<Review>> GetByUserIdAsync(Guid userId) =>
        await _db.Reviews
            .Include(r => r.Template)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task AddAsync(Review review)
    {
        await _db.Reviews.AddAsync(review);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Review review)
    {
        _db.Reviews.Update(review);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Review review)
    {
        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateTemplateRatingAsync(Guid templateId)
    {
        var approved = await _db.Reviews
            .Where(r => r.TemplateId == templateId && r.IsApproved)
            .ToListAsync();

        var template = await _db.Templates.FindAsync(templateId);
        if (template is null) return;

        template.AverageRating = approved.Count > 0
            ? Math.Round(approved.Average(r => r.Rating), 1)
            : 0;
        template.ReviewCount = approved.Count;

        await _db.SaveChangesAsync();
    }
    public async Task<List<Review>> GetPendingAiAsync(int limit = 10) =>
    await _db.Reviews
        .Where(r => r.AiStatus == "Pending")
        .OrderBy(r => r.CreatedAt)
        .Take(limit)
        .ToListAsync();
    public async Task<List<(Guid UserId, int Count)>> GetSpamUsersAsync(
        DateTime from, int threshold)
    {
        var rows = await _db.Reviews
            .Where(r => r.CreatedAt >= from)
            .GroupBy(r => r.UserId)
            .Where(g => g.Count() >= threshold)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToListAsync();

        return rows.Select(x => (x.UserId, x.Count)).ToList();
    }
}
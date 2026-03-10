using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class AffiliateRepository : IAffiliateRepository
{
    private readonly AppDbContext _db;
    public AffiliateRepository(AppDbContext db) => _db = db;

    public async Task<Affiliate?> GetByIdAsync(int id) =>
        await _db.Affiliates
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<Affiliate?> GetByUserIdAsync(Guid userId) =>
        await _db.Affiliates
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.UserId == userId);

    public async Task<Affiliate?> GetByCodeAsync(string code) =>
        await _db.Affiliates
            .FirstOrDefaultAsync(a => a.AffiliateCode == code && a.IsActive);

    public async Task<(List<Affiliate> Items, int Total)> GetAdminListAsync(
        bool? isActive, int page, int pageSize)
    {
        var query = _db.Affiliates
            .Include(a => a.User)
            .AsQueryable();

        if (isActive.HasValue)
            query = query.Where(a => a.IsActive == isActive.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.TotalEarned)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task AddAsync(Affiliate affiliate)
    {
        await _db.Affiliates.AddAsync(affiliate);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Affiliate affiliate)
    {
        _db.Affiliates.Update(affiliate);
        await _db.SaveChangesAsync();
    }

    public async Task AddTransactionAsync(AffiliateTransaction tx)
    {
        await _db.AffiliateTransactions.AddAsync(tx);
        await _db.SaveChangesAsync();
    }

    public async Task<List<AffiliateTransaction>> GetTransactionsByAffiliateIdAsync(int affiliateId) =>
        await _db.AffiliateTransactions
            .Include(t => t.Order)
            .Where(t => t.AffiliateId == affiliateId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    public async Task<AffiliateTransaction?> GetTransactionByIdAsync(int id) =>
       await _db.AffiliateTransactions
           .Include(t => t.Affiliate)
           .FirstOrDefaultAsync(t => t.Id == id);

    public async Task UpdateTransactionAsync(AffiliateTransaction tx)
    {
        _db.AffiliateTransactions.Update(tx);
        await _db.SaveChangesAsync();
    }
}
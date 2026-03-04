using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class CouponRepository : ICouponRepository
{
    private readonly AppDbContext _db;
    public CouponRepository(AppDbContext db) => _db = db;

    public async Task<Coupon?> GetByCodeAsync(string code) =>
        await _db.Coupons.FirstOrDefaultAsync(c => c.Code == code);

    public async Task<Coupon?> GetByIdAsync(int id) =>
        await _db.Coupons.FindAsync(id);

    public async Task<(List<Coupon> Items, int Total)> GetAdminListAsync(
        bool? isActive, int page, int pageSize)
    {
        var query = _db.Coupons.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task AddAsync(Coupon coupon)
    {
        await _db.Coupons.AddAsync(coupon);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Coupon coupon)
    {
        _db.Coupons.Update(coupon);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Coupon coupon)
    {
        _db.Coupons.Remove(coupon);
        await _db.SaveChangesAsync();
    }
}
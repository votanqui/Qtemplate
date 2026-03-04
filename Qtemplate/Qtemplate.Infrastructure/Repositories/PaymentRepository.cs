using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _db;
    public PaymentRepository(AppDbContext db) => _db = db;

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId) =>
        await _db.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);

    public async Task<Payment?> GetByTransferContentAsync(string transferContent) =>
        await _db.Payments
            .Include(p => p.Order).ThenInclude(o => o.Items)
            .FirstOrDefaultAsync(p => p.TransferContent == transferContent);

    public async Task AddAsync(Payment payment)
    {
        await _db.Payments.AddAsync(payment);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Payment payment)
    {
        _db.Payments.Update(payment);
        await _db.SaveChangesAsync();
    }
    public async Task<List<Payment>> GetByDateRangeAsync(DateTime from, DateTime to) =>
    await _db.Payments
        .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
        .ToListAsync();

    public async Task<List<Payment>> GetByDateRangeWithOrderAsync(DateTime from, DateTime to) =>
        await _db.Payments
            .Include(p => p.Order)
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
            .ToListAsync();
}
using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByOrderIdAsync(Guid orderId);
    Task<Payment?> GetByTransferContentAsync(string transferContent);
    Task AddAsync(Payment payment);
    Task UpdateAsync(Payment payment);
    Task<List<Payment>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<List<Payment>> GetByDateRangeWithOrderAsync(DateTime from, DateTime to);
}
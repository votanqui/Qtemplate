using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface ITicketRepository
{
    Task<SupportTicket?> GetByIdAsync(int id);
    Task<SupportTicket?> GetByIdWithRepliesAsync(int id);
    Task<(List<SupportTicket> Items, int Total)> GetByUserIdAsync(
        Guid userId, int page, int pageSize);
    Task<(List<SupportTicket> Items, int Total)> GetAdminListAsync(
        string? status, string? priority, int page, int pageSize);
    Task AddAsync(SupportTicket ticket);
    Task UpdateAsync(SupportTicket ticket);
    Task AddReplyAsync(TicketReply reply);
    Task<string> GenerateTicketCodeAsync();
    Task<List<SupportTicket>> GetPendingAiAsync(int limit = 10);
}
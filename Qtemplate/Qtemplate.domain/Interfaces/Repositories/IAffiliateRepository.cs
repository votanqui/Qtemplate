// File: Qtemplate.domain/Interfaces/Repositories/IAffiliateRepository.cs

using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface IAffiliateRepository
{
    Task<Affiliate?> GetByIdAsync(int id);
    Task<Affiliate?> GetByUserIdAsync(Guid userId);
    Task<Affiliate?> GetByCodeAsync(string code);
    Task<(List<Affiliate> Items, int Total)> GetAdminListAsync(
        bool? isActive, int page, int pageSize);
    Task AddAsync(Affiliate affiliate);
    Task UpdateAsync(Affiliate affiliate);
    Task AddTransactionAsync(AffiliateTransaction tx);
    Task<List<AffiliateTransaction>> GetTransactionsByAffiliateIdAsync(int affiliateId);
    Task<AffiliateTransaction?> GetTransactionByIdAsync(int id);
    Task UpdateTransactionAsync(AffiliateTransaction tx);

    // ── Paged transactions ────────────────────────────────────────────────────
    Task<(List<AffiliateTransaction> Items, int Total)> GetMyTransactionsPagedAsync(
        Guid userId, string? status, int page, int pageSize);

    Task<(Affiliate? Affiliate, List<AffiliateTransaction> Items, int Total)> GetAdminTransactionsPagedAsync(
        int affiliateId, string? status, int page, int pageSize);
}
using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class TemplateVersionRepository : ITemplateVersionRepository
{
    private readonly AppDbContext _db;
    public TemplateVersionRepository(AppDbContext db) => _db = db;

    public async Task<List<TemplateVersion>> GetByTemplateIdAsync(Guid templateId) =>
        await _db.TemplateVersions
            .Where(v => v.TemplateId == templateId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

    public async Task<TemplateVersion?> GetByIdAsync(int id) =>
        await _db.TemplateVersions.FindAsync(id);

    public async Task AddAsync(TemplateVersion version)
    {
        await _db.TemplateVersions.AddAsync(version);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(TemplateVersion version)
    {
        _db.TemplateVersions.Update(version);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(TemplateVersion version)
    {
        _db.TemplateVersions.Remove(version);
        await _db.SaveChangesAsync();
    }
}
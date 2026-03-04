using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class TemplateImageRepository : ITemplateImageRepository
{
    private readonly AppDbContext _db;
    public TemplateImageRepository(AppDbContext db) => _db = db;

    public async Task<TemplateImage?> GetByIdAsync(int id) =>
        await _db.TemplateImages.FindAsync(id);

    public async Task AddAsync(TemplateImage image)
    {
        await _db.TemplateImages.AddAsync(image);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(TemplateImage image)
    {
        _db.TemplateImages.Remove(image);
        await _db.SaveChangesAsync();
    }
    public async Task<List<TemplateImage>> GetByTemplateIdAsync(Guid templateId) =>
    await _db.TemplateImages
        .Where(i => i.TemplateId == templateId)
        .ToListAsync();
}
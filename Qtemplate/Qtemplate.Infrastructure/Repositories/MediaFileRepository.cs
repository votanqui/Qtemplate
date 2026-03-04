using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class MediaFileRepository : IMediaFileRepository
{
    private readonly AppDbContext _db;
    public MediaFileRepository(AppDbContext db) => _db = db;

    public async Task<MediaFile?> GetByIdAsync(int id) =>
        await _db.MediaFiles.FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

    public async Task<(List<MediaFile> Items, int Total)> GetListAsync(
        Guid? templateId, int page, int pageSize)
    {
        var query = _db.MediaFiles
            .Include(m => m.Template)
            .Where(m => !m.IsDeleted)
            .AsQueryable();

        if (templateId.HasValue)
            query = query.Where(m => m.TemplateId == templateId);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task AddAsync(MediaFile mediaFile)
    {
        await _db.MediaFiles.AddAsync(mediaFile);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(MediaFile mediaFile)
    {
        _db.MediaFiles.Update(mediaFile);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(MediaFile mediaFile)
    {
        _db.MediaFiles.Remove(mediaFile);
        await _db.SaveChangesAsync();
    }
    public async Task<List<MediaFile>> GetAllAsync() =>
    await _db.MediaFiles.AsNoTracking().ToListAsync();
}
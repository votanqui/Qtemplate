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
        var query = from m in _db.MediaFiles
                    where !m.IsDeleted
                    join t in _db.Templates on m.TemplateId equals t.Id into tg
                    from t in tg.DefaultIfEmpty()   // LEFT JOIN
                    select new { m, TemplateName = t != null ? t.Name : null };

        if (templateId.HasValue)
            query = query.Where(x => x.m.TemplateId == templateId);

        var total = await query.CountAsync();

        var rows = await query
            .OrderByDescending(x => x.m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Gán TemplateName vào navigation stub để ToDto dùng m.Template?.Name
        var items = rows.Select(x =>
        {
            x.m.Template = x.TemplateName != null
                ? new Template { Name = x.TemplateName }
                : null;
            return x.m;
        }).ToList();

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
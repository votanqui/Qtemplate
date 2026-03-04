using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class TagRepository : ITagRepository
{
    private readonly AppDbContext _db;
    public TagRepository(AppDbContext db) => _db = db;

    public async Task<List<Tag>> GetAllAsync() =>
        await _db.Tags.OrderBy(t => t.Name).ToListAsync();

    public async Task<Tag?> GetByIdAsync(int id) =>
        await _db.Tags.FindAsync(id);

    public async Task<bool> SlugExistsAsync(string slug) =>
        await _db.Tags.AnyAsync(t => t.Slug == slug);

    public async Task AddAsync(Tag tag)
    {
        await _db.Tags.AddAsync(tag);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Tag tag)
    {
        _db.Tags.Update(tag);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Tag tag)
    {
        _db.Tags.Remove(tag);
        await _db.SaveChangesAsync();
    }
}
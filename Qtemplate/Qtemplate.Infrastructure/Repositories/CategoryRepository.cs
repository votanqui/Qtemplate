using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;
    public CategoryRepository(AppDbContext db) => _db = db;

    public async Task<List<Category>> GetAllAsync() =>
        await _db.Categories
            .Include(c => c.Children)
            .Where(c => c.ParentId == null)   // chỉ lấy root, Children tự load
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

    public async Task<Category?> GetByIdAsync(int id) =>
        await _db.Categories.Include(c => c.Children).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<bool> SlugExistsAsync(string slug) =>
        await _db.Categories.AnyAsync(c => c.Slug == slug);

    public async Task AddAsync(Category category)
    {
        await _db.Categories.AddAsync(category);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        _db.Categories.Update(category);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Category category)
    {
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
    }
}
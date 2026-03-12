using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class SettingRepository : ISettingRepository
{
    private readonly AppDbContext _db;
    public SettingRepository(AppDbContext db) => _db = db;

    public async Task<string?> GetValueAsync(string key) =>
        (await _db.Settings.FirstOrDefaultAsync(s => s.Key == key))?.Value;

    public async Task<Dictionary<string, string>> GetGroupAsync(string? group = null) =>
        await _db.Settings
            .Where(s => s.Value != null && (group == null || s.Group == group))
            .ToDictionaryAsync(s => s.Key, s => s.Value!);
    public async Task<List<Setting>> GetDetailAsync(string? group = null) =>
    await _db.Settings
        .Where(s => group == null || s.Group == group)
        .OrderBy(s => s.Group).ThenBy(s => s.Key)
        .ToListAsync();
    public async Task SetValueAsync(string key, string value, string group = "General", string? description = null)
    {
        var setting = await _db.Settings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting is null)
        {
            await _db.Settings.AddAsync(new Setting
            {
                Key = key,
                Value = value,
                Group = group,
                Description = description,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
    }
    public async Task<int> GetIntAsync(string key, int defaultValue = 0)
    {
        var value = await GetValueAsync(key);
        return int.TryParse(value, out var result) ? result : defaultValue;
    }
}
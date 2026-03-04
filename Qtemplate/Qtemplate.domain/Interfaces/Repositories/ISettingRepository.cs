using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface ISettingRepository
{
    Task<string?> GetValueAsync(string key);
    Task<Dictionary<string, string>> GetGroupAsync(string? group = null);
    Task<List<Setting>> GetDetailAsync(string? group = null);  // trả Entity
    Task SetValueAsync(string key, string value, string group = "General", string? description = null);
}
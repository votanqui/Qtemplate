using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface IMediaFileRepository
{
    Task<MediaFile?> GetByIdAsync(int id);
    Task<(List<MediaFile> Items, int Total)> GetListAsync(Guid? templateId, int page, int pageSize);
    Task<List<MediaFile>> GetAllAsync();   // ← thêm
    Task AddAsync(MediaFile mediaFile);
    Task UpdateAsync(MediaFile mediaFile);
    Task DeleteAsync(MediaFile mediaFile);
}
using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface IUserDownloadRepository
{
    Task<(List<UserDownload>, int)> GetPagedByUserIdAsync(Guid userId, int page, int pageSize);
    Task UpsertAsync(Guid userId, Guid templateId, Guid orderId,
                     string? ip = null, string? userAgent = null);
}
using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface ITemplateVersionRepository
{
    Task<List<TemplateVersion>> GetByTemplateIdAsync(Guid templateId);
    Task<TemplateVersion?> GetByIdAsync(int id);
    Task AddAsync(TemplateVersion version);
    Task UpdateAsync(TemplateVersion version);
    Task DeleteAsync(TemplateVersion version);
}
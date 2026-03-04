using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface ITemplateImageRepository
{
    Task<TemplateImage?> GetByIdAsync(int id);
    Task AddAsync(TemplateImage image);
    Task DeleteAsync(TemplateImage image);
    Task<List<TemplateImage>> GetByTemplateIdAsync(Guid templateId);
}
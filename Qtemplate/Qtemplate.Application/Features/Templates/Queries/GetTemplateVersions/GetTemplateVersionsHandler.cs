using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Template.Admin;
using Qtemplate.Application.Mappers;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Queries.GetTemplateVersions;

public class GetTemplateVersionsHandler
    : IRequestHandler<GetTemplateVersionsQuery, ApiResponse<List<TemplateVersionDto>>>
{
    private readonly ITemplateVersionRepository _versionRepo;
    public GetTemplateVersionsHandler(ITemplateVersionRepository versionRepo) => _versionRepo = versionRepo;

    public async Task<ApiResponse<List<TemplateVersionDto>>> Handle(
        GetTemplateVersionsQuery request, CancellationToken cancellationToken)
    {
        var versions = await _versionRepo.GetByTemplateIdAsync(request.TemplateId);
        return ApiResponse<List<TemplateVersionDto>>.Ok(
            versions.Select(TemplateMapper.ToVersionDto).ToList());
    }
}
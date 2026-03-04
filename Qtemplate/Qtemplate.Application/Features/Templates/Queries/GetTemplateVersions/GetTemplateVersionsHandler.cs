// Queries/GetTemplateVersions/GetTemplateVersionsHandler.cs
using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Template.Admin;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Queries.GetTemplateVersions;

public class GetTemplateVersionsHandler : IRequestHandler<GetTemplateVersionsQuery, ApiResponse<List<TemplateVersionDto>>>
{
    private readonly ITemplateVersionRepository _versionRepo;
    public GetTemplateVersionsHandler(ITemplateVersionRepository versionRepo) => _versionRepo = versionRepo;

    public async Task<ApiResponse<List<TemplateVersionDto>>> Handle(GetTemplateVersionsQuery request, CancellationToken cancellationToken)
    {
        var versions = await _versionRepo.GetByTemplateIdAsync(request.TemplateId);
        var dtos = versions.Select(v => new TemplateVersionDto
        {
            Id = v.Id,
            Version = v.Version,
            ChangeLog = v.ChangeLog,
            IsLatest = v.IsLatest,
            CreatedAt = v.CreatedAt
        }).ToList();
        return ApiResponse<List<TemplateVersionDto>>.Ok(dtos);
    }
}
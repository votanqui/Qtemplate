// Queries/GetTemplateVersions/GetTemplateVersionsQuery.cs
using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Template.Admin;

namespace Qtemplate.Application.Features.Templates.Queries.GetTemplateVersions;

public class GetTemplateVersionsQuery : IRequest<ApiResponse<List<TemplateVersionDto>>>
{
    public Guid TemplateId { get; set; }
}
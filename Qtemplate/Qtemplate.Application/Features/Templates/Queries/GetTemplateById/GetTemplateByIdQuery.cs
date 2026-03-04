using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Entities;

namespace Qtemplate.Application.Features.Templates.Queries.GetTemplateById;

public class GetTemplateByIdQuery : IRequest<ApiResponse<Template>>
{
    public Guid Id { get; set; }
}
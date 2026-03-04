using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Queries.GetTemplateById;

public class GetTemplateByIdHandler : IRequestHandler<GetTemplateByIdQuery, ApiResponse<Template>>
{
    private readonly ITemplateRepository _templateRepo;

    public GetTemplateByIdHandler(ITemplateRepository templateRepo) => _templateRepo = templateRepo;

    public async Task<ApiResponse<Template>> Handle(GetTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await _templateRepo.GetByIdAsync(request.Id);
        if (template is null)
            return ApiResponse<Template>.Fail("Không tìm thấy template");

        return ApiResponse<Template>.Ok(template);
    }
}
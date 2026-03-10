using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Template.Admin;
using Qtemplate.Application.Mappers;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Queries.AdminGetTemplates;

public class AdminGetTemplatesHandler
    : IRequestHandler<AdminGetTemplatesQuery, ApiResponse<PaginatedResult<AdminTemplateListDto>>>
{
    private readonly ITemplateRepository _templateRepo;
    public AdminGetTemplatesHandler(ITemplateRepository templateRepo) => _templateRepo = templateRepo;

    public async Task<ApiResponse<PaginatedResult<AdminTemplateListDto>>> Handle(
        AdminGetTemplatesQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _templateRepo.GetAdminListAsync(
            request.Search, request.Status, request.CategoryId,
            request.Page, request.PageSize);

        return ApiResponse<PaginatedResult<AdminTemplateListDto>>.Ok(new PaginatedResult<AdminTemplateListDto>
        {
            Items = items.Select(TemplateMapper.ToAdminListDto).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
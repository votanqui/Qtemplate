using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Template;
using Qtemplate.Application.Mappers;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Queries.GetTemplates;

public class GetTemplatesHandler : IRequestHandler<GetTemplatesQuery, ApiResponse<PaginatedResult<TemplateListDto>>>
{
    private readonly ITemplateRepository _templateRepo;
    private readonly IWishlistRepository _wishlistRepo;

    public GetTemplatesHandler(
        ITemplateRepository templateRepo,
        IWishlistRepository wishlistRepo)
    {
        _templateRepo = templateRepo;
        _wishlistRepo = wishlistRepo;
    }

    public async Task<ApiResponse<PaginatedResult<TemplateListDto>>> Handle(
        GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _templateRepo.GetPublicListAsync(
            search: request.Search,
            categorySlug: request.CategorySlug,
            tagSlug: request.TagSlug,
            isFree: request.IsFree,
            minPrice: request.MinPrice,
            maxPrice: request.MaxPrice,
            sortBy: request.SortBy,
            page: request.Page,
            pageSize: request.PageSize,
            onSale: request.OnSale,
            isFeatured: request.IsFeatured,
            isNew: request.IsNew,
            techStack: request.TechStack);

        HashSet<Guid> wishlistIds = new();
        if (request.CurrentUserId.HasValue)
        {
            var ids = await _wishlistRepo.GetTemplateIdsByUserAsync(request.CurrentUserId.Value);
            wishlistIds = ids.ToHashSet();
        }

        return ApiResponse<PaginatedResult<TemplateListDto>>.Ok(new PaginatedResult<TemplateListDto>
        {
            Items = items.Select(t => TemplateMapper.ToListDto(t, wishlistIds.Contains(t.Id))).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
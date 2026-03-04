using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Template;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Queries.GetTemplates;

public class GetTemplatesHandler : IRequestHandler<GetTemplatesQuery, ApiResponse<PaginatedResult<TemplateListDto>>>
{
    private readonly ITemplateRepository _templateRepo;
    private readonly IWishlistRepository _wishlistRepo;

    public GetTemplatesHandler(ITemplateRepository templateRepo, IWishlistRepository wishlistRepo)
    {
        _templateRepo = templateRepo;
        _wishlistRepo = wishlistRepo;
    }

    public async Task<ApiResponse<PaginatedResult<TemplateListDto>>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
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
            pageSize: request.PageSize);

        HashSet<Guid> wishlistIds = new();
        if (request.CurrentUserId.HasValue)
        {
            var ids = await _wishlistRepo.GetTemplateIdsByUserAsync(request.CurrentUserId.Value);
            wishlistIds = ids.ToHashSet();
        }

        var dtos = items.Select(t => new TemplateListDto
        {
            Id = t.Id,
            Name = t.Name,
            Slug = t.Slug,
            ShortDescription = t.ShortDescription,
            ThumbnailUrl = t.ThumbnailUrl,
            Price = t.Price,
            SalePrice = t.SalePrice,
            IsFree = t.IsFree,
            IsFeatured = t.IsFeatured,
            IsNew = t.IsNew,
            PreviewType = t.PreviewType,
            SalesCount = t.SalesCount,
            ViewCount = t.ViewCount,
            AverageRating = t.AverageRating,
            ReviewCount = t.ReviewCount,
            CategoryName = t.Category.Name,
            CategorySlug = t.Category.Slug,
            Tags = t.TemplateTags.Select(tt => tt.Tag.Name).ToList(),
            CreatedAt = t.CreatedAt,
            IsInWishlist = wishlistIds.Contains(t.Id)
        }).ToList();

        return ApiResponse<PaginatedResult<TemplateListDto>>.Ok(new PaginatedResult<TemplateListDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
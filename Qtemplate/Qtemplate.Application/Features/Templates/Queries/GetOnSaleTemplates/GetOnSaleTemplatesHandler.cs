using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Template;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Queries.GetOnSaleTemplates;

public class GetOnSaleTemplatesHandler
    : IRequestHandler<GetOnSaleTemplatesQuery, ApiResponse<PaginatedResult<SaleTemplateDto>>>
{
    private readonly ITemplateRepository _templateRepo;
    private readonly IWishlistRepository _wishlistRepo;

    public GetOnSaleTemplatesHandler(
        ITemplateRepository templateRepo,
        IWishlistRepository wishlistRepo)
    {
        _templateRepo = templateRepo;
        _wishlistRepo = wishlistRepo;
    }

    public async Task<ApiResponse<PaginatedResult<SaleTemplateDto>>> Handle(
        GetOnSaleTemplatesQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _templateRepo.GetOnSaleListAsync(
            search: request.Search,
            categorySlug: request.CategorySlug,
            page: request.Page,
            pageSize: request.PageSize);

        HashSet<Guid> wishlistIds = new();
        if (request.CurrentUserId.HasValue)
        {
            var ids = await _wishlistRepo.GetTemplateIdsByUserAsync(request.CurrentUserId.Value);
            wishlistIds = ids.ToHashSet();
        }

        var dtos = items.Select(t => ToSaleDto(t, wishlistIds.Contains(t.Id))).ToList();

        return ApiResponse<PaginatedResult<SaleTemplateDto>>.Ok(new PaginatedResult<SaleTemplateDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }

    private static SaleTemplateDto ToSaleDto(Template t, bool isInWishlist)
    {
        var salePrice = t.SalePrice!.Value;
        var discountPercent = (int)Math.Round((t.Price - salePrice) / t.Price * 100);

        return new SaleTemplateDto
        {
            Id = t.Id,
            Name = t.Name,
            Slug = t.Slug,
            ShortDescription = t.ShortDescription,
            ThumbnailUrl = t.ThumbnailUrl,
            OriginalPrice = t.Price,
            SalePrice = salePrice,
            DiscountPercent = discountPercent,
            SaveAmount = t.Price - salePrice,
            SaleStartAt = t.SaleStartAt,
            SaleEndAt = t.SaleEndAt,
            IsOpenEnded = t.SaleEndAt == null,
            PreviewType = t.PreviewType,
            SalesCount = t.SalesCount,
            ViewCount = t.ViewCount,
            AverageRating = t.AverageRating,
            ReviewCount = t.ReviewCount,
            IsFeatured = t.IsFeatured,
            IsNew = t.IsNew,
            CategoryName = t.Category?.Name ?? string.Empty,
            CategorySlug = t.Category?.Slug ?? string.Empty,
            Tags = t.TemplateTags?.Select(tt => tt.Tag.Name).ToList() ?? new(),
            IsInWishlist = isInWishlist
        };
    }
}
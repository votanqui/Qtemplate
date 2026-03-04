using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Template;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Templates.Queries.GetTemplateDetail;

public class GetTemplateDetailHandler : IRequestHandler<GetTemplateDetailQuery, ApiResponse<TemplateDetailDto>>
{
    private readonly ITemplateRepository _templateRepo;
    private readonly IWishlistRepository _wishlistRepo;
    private readonly IOrderRepository _orderRepo;

    public GetTemplateDetailHandler(
        ITemplateRepository templateRepo,
        IWishlistRepository wishlistRepo,
        IOrderRepository orderRepo)
    {
        _templateRepo = templateRepo;
        _wishlistRepo = wishlistRepo;
        _orderRepo = orderRepo;
    }

    public async Task<ApiResponse<TemplateDetailDto>> Handle(GetTemplateDetailQuery request, CancellationToken cancellationToken)
    {
        var t = await _templateRepo.GetBySlugAsync(request.Slug);
        if (t is null)
            return ApiResponse<TemplateDetailDto>.Fail("Không tìm thấy template");

        // Public chỉ xem Published, admin xem tất cả
        if (!request.IsAdmin && t.Status != "Published")
            return ApiResponse<TemplateDetailDto>.Fail("Không tìm thấy template");

        // Chỉ tăng view khi public xem, không tăng khi admin xem
        if (!request.IsAdmin)
            await _templateRepo.IncrementViewCountAsync(t.Id);

        var isInWishlist = false;
        var isPurchased = false;

        if (request.CurrentUserId.HasValue && !request.IsAdmin)
        {
            isInWishlist = await _wishlistRepo.ExistsAsync(request.CurrentUserId.Value, t.Id);
            isPurchased = await _orderRepo.HasPurchasedAsync(request.CurrentUserId.Value, t.Id);
        }

        return ApiResponse<TemplateDetailDto>.Ok(new TemplateDetailDto
        {
            Id = t.Id,
            Name = t.Name,
            Slug = t.Slug,
            ShortDescription = t.ShortDescription,
            Description = t.Description,
            ChangeLog = t.ChangeLog,
            Price = t.Price,
            SalePrice = t.SalePrice,
            SaleStartAt = t.SaleStartAt,
            SaleEndAt = t.SaleEndAt,
            Status = t.Status,
            ThumbnailUrl = t.ThumbnailUrl,
            PreviewType = t.PreviewType,
            PreviewFolder = t.PreviewType == "Iframe" ? t.PreviewFolder : null,
            PreviewUrl = t.PreviewType == "ExternalUrl" ? t.PreviewUrl : null,
            DownloadPath = isPurchased ? t.DownloadPath : null, // chỉ trả nếu đã mua
            TechStack = t.TechStack,
            CompatibleWith = t.CompatibleWith,
            FileFormat = t.FileFormat,
            Version = t.Version,
            IsFeatured = t.IsFeatured,
            IsNew = t.IsNew,
            IsFree = t.IsFree,
            ViewCount = t.ViewCount,
            SalesCount = t.SalesCount,
            WishlistCount = t.WishlistCount,
            AverageRating = t.AverageRating,
            ReviewCount = t.ReviewCount,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            PublishedAt = t.PublishedAt,
            Category = new TemplateCategoryDto
            {
                Id = t.Category.Id,
                Name = t.Category.Name,
                Slug = t.Category.Slug
            },
            Tags = t.TemplateTags.Select(tt => tt.Tag.Name).ToList(),
            Features = t.Features.OrderBy(f => f.SortOrder).Select(f => f.Feature).ToList(),
            Images = t.Images.OrderBy(i => i.SortOrder).Select(i => new TemplateImageDto
            {
                ImageUrl = i.ImageUrl,
                AltText = i.AltText,
                Type = i.Type,
                SortOrder = i.SortOrder
            }).ToList(),
            IsInWishlist = isInWishlist,
            IsPurchased = isPurchased
        });
    }
}
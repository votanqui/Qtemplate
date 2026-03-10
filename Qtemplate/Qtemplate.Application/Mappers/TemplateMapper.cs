using Qtemplate.Application.DTOs.Template;
using Qtemplate.Application.DTOs.Template.Admin;
using Qtemplate.Domain.Entities;

namespace Qtemplate.Application.Mappers;

public static class TemplateMapper
{
    // ── Public list ──────────────────────────────────────────────────────────
    public static TemplateListDto ToListDto(Template t, bool isInWishlist = false) => new()
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
        CategoryName = t.Category?.Name ?? string.Empty,
        CategorySlug = t.Category?.Slug ?? string.Empty,
        Tags = t.TemplateTags?.Select(tt => tt.Tag.Name).ToList() ?? new(),
        CreatedAt = t.CreatedAt,
        IsInWishlist = isInWishlist
    };

    // ── Public detail ────────────────────────────────────────────────────────
    public static TemplateDetailDto ToDetailDto(
        Template t,
        bool isInWishlist = false,
        bool isPurchased = false) => new()
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
            // Chỉ trả PreviewFolder khi Iframe, PreviewUrl khi ExternalUrl
            PreviewFolder = t.PreviewType == "Iframe" ? t.PreviewFolder : null,
            PreviewUrl = t.PreviewType == "ExternalUrl" ? t.PreviewUrl : null,
            // Chỉ trả DownloadPath khi đã mua
            DownloadPath = isPurchased ? t.DownloadPath : null,
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
            Category = t.Category is null ? new() : new TemplateCategoryDto
            {
                Id = t.Category.Id,
                Name = t.Category.Name,
                Slug = t.Category.Slug
            },
            Tags = t.TemplateTags?.Select(tt => tt.Tag.Name).ToList() ?? new(),
            Features = t.Features?
            .OrderBy(f => f.SortOrder)
            .Select(f => f.Feature)
            .ToList() ?? new(),
            Images = t.Images?
            .OrderBy(i => i.SortOrder)
            .Select(ToImageDto)
            .ToList() ?? new(),
            IsInWishlist = isInWishlist,
            IsPurchased = isPurchased
        };

    // ── Admin list ───────────────────────────────────────────────────────────
    public static AdminTemplateListDto ToAdminListDto(Template t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Slug = t.Slug,
        Status = t.Status,
        Price = t.Price,
        ThumbnailUrl = t.ThumbnailUrl,
        SalePrice = t.SalePrice,
        IsFree = t.IsFree,
        IsFeatured = t.IsFeatured,
        SalesCount = t.SalesCount,
        ViewCount = t.ViewCount,
        AverageRating = t.AverageRating,
        CategoryName = t.Category?.Name ?? string.Empty,
        CreatedAt = t.CreatedAt,
        PublishedAt = t.PublishedAt
    };

    // ── Version ──────────────────────────────────────────────────────────────
    public static TemplateVersionDto ToVersionDto(TemplateVersion v) => new()
    {
        Id = v.Id,
        Version = v.Version,
        ChangeLog = v.ChangeLog,
        IsLatest = v.IsLatest,
        CreatedAt = v.CreatedAt
    };

    // ── Image ────────────────────────────────────────────────────────────────
    public static TemplateImageDto ToImageDto(TemplateImage i) => new()
    {
        ImageUrl = i.ImageUrl,
        AltText = i.AltText,
        Type = i.Type,
        SortOrder = i.SortOrder
    };
}

using MediatR;
using Qtemplate.Application.DTOs.Wishlist;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.UserManagement.Queries.Wishlist;

public class GetWishlistHandler : IRequestHandler<GetWishlistQuery, ApiResponse<PaginatedResult<WishlistItemDto>>>
{
    private readonly IWishlistRepository _wishlistRepo;
    public GetWishlistHandler(IWishlistRepository wishlistRepo) => _wishlistRepo = wishlistRepo;

    public async Task<ApiResponse<PaginatedResult<WishlistItemDto>>> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _wishlistRepo.GetPagedByUserIdAsync(request.UserId, request.Page, request.PageSize);

        var dtos = items.Select(w => new WishlistItemDto
        {
            Id = w.Id,
            TemplateId = w.TemplateId,
            TemplateName = w.Template.Name,
            TemplateSlug = w.Template.Slug,
            ThumbnailUrl = w.Template.ThumbnailUrl,
            Price = w.Template.Price,
            SalePrice = w.Template.SalePrice,
            IsFree = w.Template.IsFree,
            Status = w.Template.Status,
            CreatedAt = w.CreatedAt
        }).ToList();

        return ApiResponse<PaginatedResult<WishlistItemDto>>.Ok(new PaginatedResult<WishlistItemDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
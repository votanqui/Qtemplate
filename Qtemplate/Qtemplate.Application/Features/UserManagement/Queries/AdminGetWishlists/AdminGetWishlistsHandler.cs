using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Wishlist;
using Qtemplate.Application.Mappers;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.UserManagement.Queries.AdminGetWishlists;

public class AdminGetWishlistsHandler
    : IRequestHandler<AdminGetWishlistsQuery, ApiResponse<PaginatedResult<AdminWishlistItemDto>>>
{
    private readonly IWishlistRepository _wishlistRepo;
    public AdminGetWishlistsHandler(IWishlistRepository wishlistRepo) => _wishlistRepo = wishlistRepo;

    public async Task<ApiResponse<PaginatedResult<AdminWishlistItemDto>>> Handle(
        AdminGetWishlistsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _wishlistRepo.GetPagedAdminAsync(
            request.UserId, request.TemplateId, request.Page, request.PageSize);

        return ApiResponse<PaginatedResult<AdminWishlistItemDto>>.Ok(new PaginatedResult<AdminWishlistItemDto>
        {
            Items = items.Select(UserMapper.ToAdminWishlistDto).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
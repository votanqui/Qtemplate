using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Wishlist;
using Qtemplate.Application.Mappers;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.UserManagement.Queries.Wishlist;

public class GetWishlistHandler
    : IRequestHandler<GetWishlistQuery, ApiResponse<PaginatedResult<WishlistItemDto>>>
{
    private readonly IWishlistRepository _wishlistRepo;
    public GetWishlistHandler(IWishlistRepository wishlistRepo) => _wishlistRepo = wishlistRepo;

    public async Task<ApiResponse<PaginatedResult<WishlistItemDto>>> Handle(
        GetWishlistQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _wishlistRepo.GetPagedByUserIdAsync(
            request.UserId, request.Page, request.PageSize);

        return ApiResponse<PaginatedResult<WishlistItemDto>>.Ok(new PaginatedResult<WishlistItemDto>
        {
            Items = items.Select(UserMapper.ToWishlistDto).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
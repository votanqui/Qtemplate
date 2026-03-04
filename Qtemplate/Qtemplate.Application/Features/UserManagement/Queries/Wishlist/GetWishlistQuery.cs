using MediatR;

using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Wishlist;

namespace Qtemplate.Application.Features.UserManagement.Queries.Wishlist
{
    public class GetWishlistQuery : IRequest<ApiResponse<PaginatedResult<WishlistItemDto>>>
    {
        public Guid UserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}

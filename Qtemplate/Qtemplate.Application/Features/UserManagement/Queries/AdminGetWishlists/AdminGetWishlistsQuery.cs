using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Wishlist;

namespace Qtemplate.Application.Features.UserManagement.Queries.AdminGetWishlists
{
    public class AdminGetWishlistsQuery : IRequest<ApiResponse<PaginatedResult<AdminWishlistItemDto>>>
    {
        public Guid? UserId { get; set; }  // filter theo user cụ thể
        public Guid? TemplateId { get; set; }  // filter theo template cụ thể
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

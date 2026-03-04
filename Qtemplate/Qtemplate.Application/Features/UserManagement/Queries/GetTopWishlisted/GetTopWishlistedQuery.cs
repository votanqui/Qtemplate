using MediatR;
using Qtemplate.Application.DTOs.Wishlist;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.UserManagement.Queries.GetTopWishlisted
{
    public class GetTopWishlistedQuery : IRequest<ApiResponse<List<TopWishlistedDto>>>
    {
        public int Top { get; set; } = 10;
    }
}

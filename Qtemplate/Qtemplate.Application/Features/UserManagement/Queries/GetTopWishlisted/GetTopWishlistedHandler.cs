using MediatR;
using Qtemplate.Application.DTOs.Wishlist;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.UserManagement.Queries.GetTopWishlisted
{
    public class GetTopWishlistedHandler : IRequestHandler<GetTopWishlistedQuery, ApiResponse<List<TopWishlistedDto>>>
    {
        private readonly IWishlistRepository _wishlistRepo;
        public GetTopWishlistedHandler(IWishlistRepository wishlistRepo) => _wishlistRepo = wishlistRepo;

        public async Task<ApiResponse<List<TopWishlistedDto>>> Handle(GetTopWishlistedQuery request, CancellationToken cancellationToken)
        {
            var top = await _wishlistRepo.GetTopWishlistedAsync(request.Top);
            var dtos = top.Select(x => new TopWishlistedDto
            {
                TemplateId = x.TemplateId,
                TemplateName = x.TemplateName,
                Count = x.Count
            }).ToList();
            return ApiResponse<List<TopWishlistedDto>>.Ok(dtos);
        }
    }
}

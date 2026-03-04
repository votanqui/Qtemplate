using MediatR;
using Qtemplate.Application.DTOs.Wishlist;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.UserManagement.Queries.AdminGetWishlists
{
    public class AdminGetWishlistsHandler : IRequestHandler<AdminGetWishlistsQuery, ApiResponse<PaginatedResult<AdminWishlistItemDto>>>
    {
        private readonly IWishlistRepository _wishlistRepo;
        public AdminGetWishlistsHandler(IWishlistRepository wishlistRepo) => _wishlistRepo = wishlistRepo;

        public async Task<ApiResponse<PaginatedResult<AdminWishlistItemDto>>> Handle(
            AdminGetWishlistsQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _wishlistRepo.GetPagedAdminAsync(
                request.UserId, request.TemplateId, request.Page, request.PageSize);

            var dtos = items.Select(w => new AdminWishlistItemDto
            {
                Id = w.Id,
                UserId = w.UserId,
                UserEmail = w.User.Email,
                TemplateId = w.TemplateId,
                TemplateName = w.Template.Name,
                CreatedAt = w.CreatedAt
            }).ToList();

            return ApiResponse<PaginatedResult<AdminWishlistItemDto>>.Ok(new PaginatedResult<AdminWishlistItemDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
    }
}

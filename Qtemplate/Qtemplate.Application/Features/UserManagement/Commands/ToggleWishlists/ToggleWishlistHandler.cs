using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.UserManagement.Commands.ToggleWishlists
{
    public class ToggleWishlistHandler : IRequestHandler<ToggleWishlistCommand, ApiResponse<bool>>
    {
        private readonly IWishlistRepository _wishlistRepo;
        private readonly ITemplateRepository _templateRepo;

        public ToggleWishlistHandler(IWishlistRepository wishlistRepo, ITemplateRepository templateRepo)
        {
            _wishlistRepo = wishlistRepo;
            _templateRepo = templateRepo;
        }

        public async Task<ApiResponse<bool>> Handle(ToggleWishlistCommand request, CancellationToken cancellationToken)
        {
            var template = await _templateRepo.GetByIdAsync(request.TemplateId);
            if (template is null || template.Status != "Published")
                return ApiResponse<bool>.Fail("Không tìm thấy template");

            var existing = await _wishlistRepo.GetAsync(request.UserId, request.TemplateId);

            if (existing is not null)
            {
                await _wishlistRepo.RemoveAsync(existing);
                template.WishlistCount = Math.Max(0, template.WishlistCount - 1);
                await _templateRepo.UpdateAsync(template);
                return ApiResponse<bool>.Ok(false, "Đã xóa khỏi wishlist");
            }

            await _wishlistRepo.AddAsync(new Wishlist
            {
                UserId = request.UserId,
                TemplateId = request.TemplateId,
                CreatedAt = DateTime.UtcNow
            });
            template.WishlistCount++;
            await _templateRepo.UpdateAsync(template);
            return ApiResponse<bool>.Ok(true, "Đã thêm vào wishlist");
        }
    }
}

using MediatR;
using Qtemplate.Application.DTOs.Banner;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Features.Banners.Queries.GetBanner;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Banners.Commands.UpdateBanner
{
    public class UpdateBannerHandler : IRequestHandler<UpdateBannerCommand, ApiResponse<BannerDto>>
    {
        private readonly IBannerRepository _repo;
        public UpdateBannerHandler(IBannerRepository repo) => _repo = repo;

        public async Task<ApiResponse<BannerDto>> Handle(
            UpdateBannerCommand request, CancellationToken cancellationToken)
        {
            var banner = await _repo.GetByIdAsync(request.Id);
            if (banner is null)
                return ApiResponse<BannerDto>.Fail("Không tìm thấy banner");

            var d = request.Data;
            var validPositions = new[] { "Home", "Sidebar", "Popup" };
            if (!validPositions.Contains(d.Position))
                return ApiResponse<BannerDto>.Fail("Position không hợp lệ: Home / Sidebar / Popup");

            banner.Title = d.Title.Trim();
            banner.SubTitle = d.SubTitle?.Trim();
            banner.ImageUrl = d.ImageUrl.Trim();
            banner.LinkUrl = d.LinkUrl?.Trim();
            banner.Position = d.Position;
            banner.SortOrder = d.SortOrder;
            banner.IsActive = d.IsActive;
            banner.StartAt = d.StartAt;
            banner.EndAt = d.EndAt;

            await _repo.UpdateAsync(banner);
            return ApiResponse<BannerDto>.Ok(GetBannersHandler.ToDto(banner), "Cập nhật banner thành công");
        }
    }
}

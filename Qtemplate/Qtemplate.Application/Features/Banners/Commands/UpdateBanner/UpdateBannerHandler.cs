using MediatR;
using Qtemplate.Application.DTOs.Banner;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Features.Banners.Queries.GetBanner;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Banners.Commands.UpdateBanner
{
    public class UpdateBannerHandler : IRequestHandler<UpdateBannerCommand, ApiResponse<BannerDto>>
    {
        private readonly IBannerRepository _repo;
        private readonly IFileUploadService _fileUploadService;

        public UpdateBannerHandler(IBannerRepository repo, IFileUploadService fileUploadService)
        {
            _repo = repo;
            _fileUploadService = fileUploadService;
        }

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

            // Nếu ảnh thay đổi → xóa ảnh cũ khỏi disk
            var oldImageUrl = banner.ImageUrl;
            var imageChanged = !string.IsNullOrWhiteSpace(d.ImageUrl)
                               && d.ImageUrl != oldImageUrl;

            if (imageChanged && !string.IsNullOrWhiteSpace(oldImageUrl))
                _fileUploadService.DeleteBannerImage(oldImageUrl);

            banner.Title = d.Title.Trim();
            banner.SubTitle = d.SubTitle?.Trim();
            banner.ImageUrl = d.ImageUrl?.Trim() ?? oldImageUrl ?? string.Empty;
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
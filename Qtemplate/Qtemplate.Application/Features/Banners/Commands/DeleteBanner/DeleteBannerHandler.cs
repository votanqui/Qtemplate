using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Banners.Commands.DeleteBanner
{
    public class DeleteBannerHandler : IRequestHandler<DeleteBannerCommand, ApiResponse<object>>
    {
        private readonly IBannerRepository _repo;
        private readonly IFileUploadService _fileUploadService;

        public DeleteBannerHandler(IBannerRepository repo, IFileUploadService fileUploadService)
        {
            _repo = repo;
            _fileUploadService = fileUploadService;
        }

        public async Task<ApiResponse<object>> Handle(
            DeleteBannerCommand request, CancellationToken cancellationToken)
        {
            var banner = await _repo.GetByIdAsync(request.Id);
            if (banner is null)
                return ApiResponse<object>.Fail("Không tìm thấy banner");

            // Xóa file ảnh khỏi disk trước khi xóa DB
            if (!string.IsNullOrWhiteSpace(banner.ImageUrl))
                _fileUploadService.DeleteBannerImage(banner.ImageUrl);

            await _repo.DeleteAsync(banner);
            return ApiResponse<object>.Ok(null!, "Đã xóa banner");
        }
    }
}
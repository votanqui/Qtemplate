using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Banners.Commands.DeleteBanner
{
    public class DeleteBannerHandler : IRequestHandler<DeleteBannerCommand, ApiResponse<object>>
    {
        private readonly IBannerRepository _repo;
        public DeleteBannerHandler(IBannerRepository repo) => _repo = repo;

        public async Task<ApiResponse<object>> Handle(
            DeleteBannerCommand request, CancellationToken cancellationToken)
        {
            var banner = await _repo.GetByIdAsync(request.Id);
            if (banner is null)
                return ApiResponse<object>.Fail("Không tìm thấy banner");

            await _repo.DeleteAsync(banner);
            return ApiResponse<object>.Ok(null!, "Đã xóa banner");
        }
    }
}

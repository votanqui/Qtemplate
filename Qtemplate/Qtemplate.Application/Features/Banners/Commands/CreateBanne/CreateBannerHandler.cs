using MediatR;
using Qtemplate.Application.DTOs.Banner;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Features.Banners.Queries.GetBanner;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Banners.Commands.CreateBanne
{
    public class CreateBannerHandler : IRequestHandler<CreateBannerCommand, ApiResponse<BannerDto>>
    {
        private readonly IBannerRepository _repo;
        public CreateBannerHandler(IBannerRepository repo) => _repo = repo;

        public async Task<ApiResponse<BannerDto>> Handle(
            CreateBannerCommand request, CancellationToken cancellationToken)
        {
            var d = request.Data;
            if (string.IsNullOrWhiteSpace(d.Title))
                return ApiResponse<BannerDto>.Fail("Title không được để trống");

            var validPositions = new[] { "Home", "Sidebar", "Popup" };
            if (!validPositions.Contains(d.Position))
                return ApiResponse<BannerDto>.Fail("Position không hợp lệ: Home / Sidebar / Popup");

            var banner = new Banner
            {
                Title = d.Title.Trim(),
                SubTitle = d.SubTitle?.Trim(),
                ImageUrl = d.ImageUrl.Trim(),
                LinkUrl = d.LinkUrl?.Trim(),
                Position = d.Position,
                SortOrder = d.SortOrder,
                IsActive = d.IsActive,
                StartAt = d.StartAt,
                EndAt = d.EndAt,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(banner);
            return ApiResponse<BannerDto>.Ok(GetBannersHandler.ToDto(banner), "Tạo banner thành công");
        }
    }

}
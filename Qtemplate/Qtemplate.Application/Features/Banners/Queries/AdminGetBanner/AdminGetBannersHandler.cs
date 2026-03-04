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

namespace Qtemplate.Application.Features.Banners.Queries.AdminGetBanner
{
    public class GetAdminBannersHandler
        : IRequestHandler<GetAdminBannersQuery, ApiResponse<PaginatedResult<BannerDto>>>
    {
        private readonly IBannerRepository _repo;
        public GetAdminBannersHandler(IBannerRepository repo) => _repo = repo;

        public async Task<ApiResponse<PaginatedResult<BannerDto>>> Handle(
            GetAdminBannersQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _repo.GetAdminListAsync(request.Page, request.PageSize);
            return ApiResponse<PaginatedResult<BannerDto>>.Ok(new PaginatedResult<BannerDto>
            {
                Items = items.Select(GetBannersHandler.ToDto).ToList(),
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
    }
}

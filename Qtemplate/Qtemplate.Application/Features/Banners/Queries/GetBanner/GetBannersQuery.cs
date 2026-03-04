using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Banner;

namespace Qtemplate.Application.Features.Banners.Queries.GetBanner;

public class GetBannersQuery : IRequest<ApiResponse<List<BannerDto>>>
{
    public string? Position { get; set; }
}


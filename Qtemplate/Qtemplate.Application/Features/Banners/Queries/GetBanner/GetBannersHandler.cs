using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Banner;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Banners.Queries.GetBanner;

public class GetBannersHandler : IRequestHandler<GetBannersQuery, ApiResponse<List<BannerDto>>>
{
    private readonly IBannerRepository _repo;
    public GetBannersHandler(IBannerRepository repo) => _repo = repo;

    public async Task<ApiResponse<List<BannerDto>>> Handle(
        GetBannersQuery request, CancellationToken cancellationToken)
    {
        var banners = await _repo.GetActiveByPositionAsync(request.Position);
        return ApiResponse<List<BannerDto>>.Ok(banners.Select(ToDto).ToList());
    }

    internal static BannerDto ToDto(Domain.Entities.Banner b) => new()
    {
        Id = b.Id,
        Title = b.Title,
        SubTitle = b.SubTitle,
        ImageUrl = b.ImageUrl,
        LinkUrl = b.LinkUrl,
        Position = b.Position,
        SortOrder = b.SortOrder,
        IsActive = b.IsActive,
        StartAt = b.StartAt,
        EndAt = b.EndAt,
        CreatedAt = b.CreatedAt
    };
}

